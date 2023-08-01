using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OoLunar.HyperSharp.Errors;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Responders
{
    public class ResponderSearcher<TInput, TOutput> where TOutput : new()
    {
        private readonly Dictionary<Type, Responder<TInput, TOutput>> _dependencies = new();
        private readonly Dictionary<Responder<TInput, TOutput>, ResponderDelegate<TInput, TOutput>> _compiledDelegates = new();
        private readonly ILogger<ResponderSearcher<TInput, TOutput>> _logger;

        public ResponderSearcher(ILogger<ResponderSearcher<TInput, TOutput>> logger) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void RegisterResponders(Assembly assembly) => RegisterResponders(assembly.GetTypes());
        public void RegisterResponders(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                if (type.IsAbstract || !type.GetInterfaces().Contains(typeof(IResponder<TInput, TOutput>)))
                {
                    HyperLogging.ResponderTypeMismatch(_logger, type, typeof(IResponder<TInput, TOutput>), null);
                    continue;
                }

                RegisterResponder(type);
            }
        }

        private void RegisterResponder(Type type)
        {
            if (_dependencies.ContainsKey(type))
            {
                HyperLogging.ResponderSkippedRegistration(_logger, type, null);
                return;
            }

            IEnumerable<Type> dependsOnAttributes = type
                .GetCustomAttributes<RequiresResponderAttribute>(inherit: true)
                .SelectMany(attribute => attribute.Dependencies);

            List<Responder<TInput, TOutput>> twigDependencies = new();
            foreach (Type? dependencyType in dependsOnAttributes)
            {
                if (!_dependencies.ContainsKey(dependencyType))
                {
                    // Register dependent twigs recursively
                    RegisterResponder(dependencyType);
                }

                twigDependencies.Add(_dependencies[dependencyType]);
            }

            Responder<TInput, TOutput> twig = new(type, twigDependencies.ToArray());
            _dependencies[type] = twig;
        }

        public Result ValidateResponders()
        {
            List<Error> errors = new();
            foreach (KeyValuePair<Type, Responder<TInput, TOutput>> branch in _dependencies)
            {
                if (branch.Key.IsAbstract || !branch.Key.GetInterfaces().Contains(typeof(IResponder<TInput, TOutput>)))
                {
                    errors.Add(new ResponderInvalidTypeError(branch.Key, branch.Key));
                }

                foreach (Responder<TInput, TOutput> twig in branch.Value.Dependencies)
                {
                    if (twig.Type.IsAbstract || !twig.Type.GetInterfaces().Contains(typeof(IResponder<TInput, TOutput>)))
                    {
                        errors.Add(new ResponderInvalidTypeError(branch.Key, twig.Type));
                    }
                    else if (!_dependencies.ContainsKey(twig.Type))
                    {
                        errors.Add(new ResponderMissingDependencyError(branch.Key, twig.Type));
                    }
                    else if (CheckRecursiveDependency(branch.Key, twig.Type))
                    {
                        errors.Add(new ResponderRecursiveDependencyError(branch.Key, twig.Type));
                    }
                }
            }

            return errors.Count != 0 ? Result.Failure(errors) : Result.Success();
        }

        public ResponderDelegate<TInput, TOutput> CompileTreeDelegate(IServiceProvider serviceProvider)
        {
            // Mark branches as dependencies if they are referenced by other branches
            foreach (Responder<TInput, TOutput> branch in _dependencies.Values)
            {
                foreach (Responder<TInput, TOutput> twig in branch.Dependencies)
                {
                    twig.IsDependancy = true;
                }

                CompileBranchDelegate(branch, serviceProvider);
            }

            // Select all branches that are not dependencies
            Dictionary<Responder<TInput, TOutput>, ResponderDelegate<TInput, TOutput>> branchDelegates = _dependencies
                .Where(branch => !branch.Value.IsDependancy)
                .ToDictionary(branch => branch.Value, branch => branch.Value.CompiledDelegate!);

            return async (context, cancellationToken) =>
            {
                Responder<TInput, TOutput> branch = null!;
                List<Error> errors = new();
                try
                {
                    foreach ((Responder<TInput, TOutput> twig, ResponderDelegate<TInput, TOutput> branchDelegate) in branchDelegates)
                    {
                        branch = twig;
                        Result<TOutput> result = await branchDelegate(context, cancellationToken);
                        if (!result.IsSuccess)
                        {
                            errors.AddRange(result.Errors);
                        }
                        else if (result.HasValue)
                        {
                            return result;
                        }
                    }
                }
                catch (Exception error)
                {
                    return Result.Failure<TOutput>(new ResponderExecutionFailedError<TInput, TOutput>(branch, error));
                }

                return Result.Failure<TOutput>(new ResponderExecutionFailedError<TInput, TOutput>(errors));
            };
        }

        private ResponderDelegate<TInput, TOutput> CompileBranchDelegate(Responder<TInput, TOutput> branch, IServiceProvider serviceProvider)
        {
            if (_compiledDelegates.TryGetValue(branch, out ResponderDelegate<TInput, TOutput>? compiledDelegate))
            {
                return compiledDelegate;
            }

            List<ResponderDelegate<TInput, TOutput>> twigDelegates = new();
            foreach (Responder<TInput, TOutput> twig in branch.Dependencies)
            {
                twigDelegates.Add(CompileBranchDelegate(twig, serviceProvider));
            }

            // Add the branch delegate last so that it's only called when all other twigs succeed.
            twigDelegates.Add(((IResponder<TInput, TOutput>)ActivatorUtilities.CreateInstance(serviceProvider, branch.Type)).RespondAsync);

            async Task<Result<TOutput>> branchDelegate(TInput context, CancellationToken cancellationToken = default)
            {
                List<Error> errors = new();
                foreach (ResponderDelegate<TInput, TOutput> twigDelegate in twigDelegates)
                {
                    Result<TOutput> result = await twigDelegate(context, cancellationToken);
                    if (!result.IsSuccess)
                    {
                        errors.AddRange(result.Errors);
                    }
                    else if (result.HasValue)
                    {
                        return result;
                    }
                }

                return Result.Failure<TOutput>(errors);
            }

            _compiledDelegates[branch] = branch.CompiledDelegate = branchDelegate;
            return branchDelegate;
        }

        private bool CheckRecursiveDependency(Type branch, Type twig)
        {
            if (twig == branch)
            {
                return true;
            }

            foreach (Responder<TInput, TOutput> dependency in _dependencies[twig].Dependencies)
            {
                if (CheckRecursiveDependency(branch, dependency.Type))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
