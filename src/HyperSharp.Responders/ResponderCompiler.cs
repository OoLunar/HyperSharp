using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.HyperSharp.Responders.Errors;
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

        public bool IsSyncronous() => _builders.TrueForAll(builder => builder.IsSyncronous);

        public ResponderDelegate<TContext, TOutput> Compile<TContext, TOutput>(IServiceProvider serviceProvider)
        {
            if (!Validate())
            {
                throw new InvalidOperationException("Cannot compile responders because there are errors.");
            }
            else if (!IsSyncronous())
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

            return (context, cancellationToken) =>
            {
                // If any of the responders throw, treat it as undefined behavior and return an error
                try
                {
                    foreach (ResponderDelegate<TContext, TOutput> responder in rootRespondersDelegates)
                    {
                        Result<TOutput> result = responder(context, cancellationToken);
                        if (!result.IsSuccess || result.HasValue)
                        {
                            return result;
                        }
                    }
                }
                catch (Exception error)
                {
                    return Result.Failure<TOutput>(new ResponderExecutionFailedError(error));
                }

                return Result.Success<TOutput>();
            };
        }

        private ResponderDelegate<TContext, TOutput> CompileDependency<TContext, TOutput>(IServiceProvider serviceProvider, ResponderBuilder builder)
        {
            IResponder<TContext, TOutput> responder = (IResponder<TContext, TOutput>)ActivatorUtilities.CreateInstance(serviceProvider, builder.Type);
            ResponderDelegate<TContext, TOutput> responderDelegate = IResponder.GetResponderDelegate(responder);
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

            return (context, cancellationToken) =>
            {
                foreach (ResponderDelegate<TContext, TOutput> dependency in dependencies)
                {
                    Result<TOutput> result = dependency(context, cancellationToken);
                    if (!result.IsSuccess || result.HasValue)
                    {
                        return result;
                    }
                }

                return Result.Success<TOutput>();
            };
        }
    }
}
