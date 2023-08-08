using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Responders
{
    public sealed class ResponderCompiler
    {
        private readonly Dictionary<Type, ResponderBuilder> _resolvedResponders = new();
        private readonly List<ResponderBuilder> _builders = new();
        private readonly ILogger<ResponderCompiler> _logger;

        public ResponderCompiler(ILogger<ResponderCompiler>? logger = null) => _logger = logger ?? NullLogger<ResponderCompiler>.Instance;

        public void Search() => Search(Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Could not find entry assembly."));
        public void Search(Assembly assembly) => Search(assembly.GetTypes());
        public void Search(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                if (type.IsAbstract || type.IsInterface || !typeof(IResponderBase).IsAssignableFrom(type))
                {
                    continue;
                }

                (Type[] needs, bool isSyncronous) = IResponderBase.RetrieveResponderMetadata(type);
                _builders.Add(new ResponderBuilder(type, isSyncronous, needs));
            }
        }

        public bool Validate()
        {
            foreach (ResponderBuilder responderBuilder in _builders)
            {
                if (!_resolvedResponders.TryAdd(responderBuilder.Type, responderBuilder))
                {
                    _logger.LogError("Responder \"{ResponderName}\" is defined more than once.", responderBuilder.Name);
                    return false;
                }
            }

            // Keep track of visited responders during DFS (depth-first search)
            HashSet<Type> visited = new();
            foreach (ResponderBuilder responderBuilder in _resolvedResponders.Values)
            {
                if (!visited.Contains(responderBuilder.Type) && HasCircularDependency(responderBuilder, visited, new HashSet<Type>()))
                {
                    _logger.LogError("Responder \"{ResponderName}\" has a circular dependency.", responderBuilder.Name);
                    return false;
                }
            }

            // All responders are valid without any circular dependencies
            return true;
        }

        private bool HasCircularDependency(ResponderBuilder currentResponder, HashSet<Type> visited, HashSet<Type> currentPath)
        {
            visited.Add(currentResponder.Type);
            currentPath.Add(currentResponder.Type);

            foreach (Type need in currentResponder.Dependencies)
            {
                if (currentPath.Contains(need))
                {
                    // Circular dependency detected
                    return true;
                }
                else if (!_resolvedResponders.TryGetValue(need, out ResponderBuilder? dependencyResponder))
                {
                    _logger.LogError("Responder \"{ResponderName}\" requires responder \"{DependencyName}\" which is not defined.", currentResponder.Name, need.FullName);
                    return false;
                }
                else if (!visited.Contains(need) && HasCircularDependency(dependencyResponder, visited, currentPath))
                {
                    return true;
                }
                else
                {
                    dependencyResponder.RequiredBy.Add(currentResponder.Type);
                }
            }

            // Remove the current responder from the current path
            currentPath.Remove(currentResponder.Type);
            return false;
        }

        public bool IsSynchronous() => _builders.TrueForAll(builder => builder.IsSyncronous);

        public ResponderDelegate<TContext, TOutput> CompileResponders<TContext, TOutput>(IServiceProvider serviceProvider)
        {
            if (!Validate())
            {
                _logger.LogError("Cannot compile responders because there are errors. Returning an empty responder.");
                return (context, cancellationToken) => Result.Failure<TOutput>("Cannot compile responders because there are errors.");
            }
            else if (!IsSynchronous())
            {
                _logger.LogWarning("Compiling syncronous responders when there are asyncronous responders.");
            }

            // Grab the responders which aren't dependencies of any other responders
            IEnumerable<ResponderBuilder> rootResponders = _builders.Where(builder => builder.RequiredBy.Count == 0);
            if (!rootResponders.Any())
            {
                return (context, cancellationToken) => Result.Success<TOutput>();
            }

            // Compile the root responders
            List<ResponderDelegate<TContext, TOutput>> rootRespondersDelegates = new();
            foreach (ResponderBuilder builder in rootResponders)
            {
                rootRespondersDelegates.Add(CompileDependency<TContext, TOutput>(serviceProvider, builder));
            }

            return rootRespondersDelegates.Count switch
            {
                1 => rootRespondersDelegates[0],
                2 => (context, cancellationToken) =>
                {
                    Result<TOutput> result = rootRespondersDelegates[0](context, cancellationToken);
                    if (!result.IsSuccess || result.HasValue)
                    {
                        return result;
                    }

                    result = rootRespondersDelegates[1](context, cancellationToken);
                    return !result.IsSuccess || result.HasValue ? result : Result.Success<TOutput>();
                }
                ,
                _ => (context, cancellationToken) =>
                {
                    foreach (ResponderDelegate<TContext, TOutput> responder in rootRespondersDelegates)
                    {
                        Result<TOutput> result = responder(context, cancellationToken);
                        if (!result.IsSuccess || result.HasValue)
                        {
                            return result;
                        }
                    }

                    return Result.Success<TOutput>();
                }
            };
        }

        private ResponderDelegate<TContext, TOutput> CompileDependency<TContext, TOutput>(IServiceProvider serviceProvider, ResponderBuilder builder)
        {
            // ActivatorUtilities throws an exception if the type has no constructors (structs)
            IResponder<TContext, TOutput> responder = builder.Type.GetConstructors().Length == 0
                ? (IResponder<TContext, TOutput>)Activator.CreateInstance(builder.Type)!
                : (IResponder<TContext, TOutput>)ActivatorUtilities.CreateInstance(serviceProvider, builder.Type);
            ResponderDelegate<TContext, TOutput> responderDelegate = responder.Respond;
            if (builder.Dependencies.Count == 0)
            {
                return responderDelegate;
            }

            List<ResponderDelegate<TContext, TOutput>> dependencies = new();
            foreach (Type dependency in builder.Dependencies)
            {
                ResponderBuilder dependencyBuilder = _resolvedResponders[dependency];
                dependencies.Add(CompileDependency<TContext, TOutput>(serviceProvider, dependencyBuilder));
            }
            dependencies.Add(responderDelegate);

            return dependencies.Count switch
            {
                1 => dependencies[0],
                2 => (context, cancellationToken) =>
                {
                    Result<TOutput> result = dependencies[0](context, cancellationToken);
                    if (!result.IsSuccess || result.HasValue)
                    {
                        return result;
                    }

                    result = dependencies[1](context, cancellationToken);
                    return !result.IsSuccess || result.HasValue ? result : Result.Success<TOutput>();
                }
                ,
                _ => (context, cancellationToken) =>
                {
                    foreach (ResponderDelegate<TContext, TOutput> responder in dependencies)
                    {
                        Result<TOutput> result = responder(context, cancellationToken);
                        if (!result.IsSuccess || result.HasValue)
                        {
                            return result;
                        }
                    }

                    return Result.Success<TOutput>();
                }
            };
        }

        public ValueResponderDelegate<TContext, TOutput> CompileAsyncResponders<TContext, TOutput>(IServiceProvider serviceProvider)
        {
            if (!Validate())
            {
                _logger.LogError("Cannot compile responders because there are errors. Returning an empty responder.");
                return (context, cancellationToken) => ValueTask.FromResult(Result.Failure<TOutput>("Cannot compile responders because there are errors."));
            }
            else if (!IsSynchronous())
            {
                _logger.LogWarning("Compiling syncronous responders when there are asyncronous responders.");
            }

            // Grab the responders which aren't dependencies of any other responders
            IEnumerable<ResponderBuilder> rootResponders = _builders.Where(builder => builder.RequiredBy.Count == 0);
            if (!rootResponders.Any())
            {
                return (context, cancellationToken) => ValueTask.FromResult(Result.Success<TOutput>());
            }

            // Compile the root responders
            List<ValueResponderDelegate<TContext, TOutput>> rootRespondersDelegates = new();
            foreach (ResponderBuilder builder in rootResponders)
            {
                rootRespondersDelegates.Add(CompileAsyncDependency<TContext, TOutput>(serviceProvider, builder));
            }

            return rootRespondersDelegates.Count switch
            {
                1 => rootRespondersDelegates[0],
                2 => async (context, cancellationToken) =>
                {
                    Result<TOutput> result = await rootRespondersDelegates[0](context, cancellationToken);
                    if (!result.IsSuccess || result.HasValue)
                    {
                        return result;
                    }

                    result = await rootRespondersDelegates[1](context, cancellationToken);
                    return !result.IsSuccess || result.HasValue ? result : Result.Success<TOutput>();
                }
                ,
                _ => async (context, cancellationToken) =>
                {
                    foreach (ValueResponderDelegate<TContext, TOutput> responder in rootRespondersDelegates)
                    {
                        Result<TOutput> result = await responder(context, cancellationToken);
                        if (!result.IsSuccess || result.HasValue)
                        {
                            return result;
                        }
                    }

                    return Result.Success<TOutput>();
                }
            };
        }

        private ValueResponderDelegate<TContext, TOutput> CompileAsyncDependency<TContext, TOutput>(IServiceProvider serviceProvider, ResponderBuilder builder)
        {
            // ActivatorUtilities throws an exception if the type has no constructors (structs)
            ValueResponderDelegate<TContext, TOutput> responderDelegate;
            if (typeof(ITaskResponder).IsAssignableFrom(builder.Type))
            {
                ITaskResponder<TContext, TOutput> taskResponder = builder.Type.GetConstructors().Length == 0
                    ? (ITaskResponder<TContext, TOutput>)Activator.CreateInstance(builder.Type)!
                    : (ITaskResponder<TContext, TOutput>)ActivatorUtilities.CreateInstance(serviceProvider, builder.Type);

                responderDelegate = async ValueTask<Result<TOutput>> (context, cancellationToken) => await taskResponder.RespondAsync(context, cancellationToken);
            }
            else
            {
                IValueResponder<TContext, TOutput> responder = builder.Type.GetConstructors().Length == 0
                    ? (IValueResponder<TContext, TOutput>)Activator.CreateInstance(builder.Type)!
                    : (IValueResponder<TContext, TOutput>)ActivatorUtilities.CreateInstance(serviceProvider, builder.Type);

                responderDelegate = responder.RespondAsync;
            }

            if (builder.Dependencies.Count == 0)
            {
                return responderDelegate;
            }

            List<ValueResponderDelegate<TContext, TOutput>> dependencies = new();
            foreach (Type dependency in builder.Dependencies)
            {
                ResponderBuilder dependencyBuilder = _resolvedResponders[dependency];
                dependencies.Add(CompileAsyncDependency<TContext, TOutput>(serviceProvider, dependencyBuilder));
            }
            dependencies.Add(responderDelegate);

            return dependencies.Count switch
            {
                1 => dependencies[0],
                2 => async (context, cancellationToken) =>
                {
                    Result<TOutput> result = await dependencies[0](context, cancellationToken);
                    if (!result.IsSuccess || result.HasValue)
                    {
                        return result;
                    }

                    result = await dependencies[1](context, cancellationToken);
                    return !result.IsSuccess || result.HasValue ? result : Result.Success<TOutput>();
                }
                ,
                _ => async (context, cancellationToken) =>
                {
                    foreach (ValueResponderDelegate<TContext, TOutput> responder in dependencies)
                    {
                        Result<TOutput> result = await responder(context, cancellationToken);
                        if (!result.IsSuccess || result.HasValue)
                        {
                            return result;
                        }
                    }

                    return Result.Success<TOutput>();
                }
            };
        }
    }
}
