using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.HyperSharp.Errors;
using OoLunar.HyperSharp.Responders;

namespace OoLunar.HyperSharp
{
    public class ResponderSearcher<TInput, TOutput> where TOutput : class
    {
        private readonly Dictionary<Type, Twig<TInput, TOutput>> _dependencies = new();
        private readonly Dictionary<Twig<TInput, TOutput>, Func<TInput, Task<Result<TOutput>>>> _compiledDelegates = new();

        public void RegisterResponders(Assembly assembly) => RegisterResponders(assembly.GetTypes());
        public void RegisterResponders(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                if (type.IsAbstract || !type.GetInterfaces().Contains(typeof(IResponder)))
                {
                    continue;
                }

                RegisterResponder(type);
            }
        }

        private void RegisterResponder(Type type)
        {
            if (_dependencies.ContainsKey(type))
            {
                return; // Responder already registered
            }

            IEnumerable<Type> dependsOnAttributes = type
                .GetCustomAttributes<DependsOnAttribute>(inherit: true)
                .SelectMany(attribute => attribute.Dependencies);

            List<Twig<TInput, TOutput>> twigDependencies = new();
            foreach (Type? dependencyType in dependsOnAttributes)
            {
                if (!_dependencies.ContainsKey(dependencyType))
                {
                    RegisterResponder(dependencyType); // Register dependent twigs recursively
                }

                twigDependencies.Add(_dependencies[dependencyType]);
            }

            Twig<TInput, TOutput> twig = new(type, twigDependencies.ToArray());
            _dependencies[type] = twig;
        }

        public Result ValidateResponders()
        {
            List<IError> errors = new();
            foreach (KeyValuePair<Type, Twig<TInput, TOutput>> branch in _dependencies)
            {
                if (branch.Key.IsAbstract || !branch.Key.GetInterfaces().Contains(typeof(IResponder)))
                {
                    errors.Add(new ResponderInvalidTypeError(branch.Key, branch.Key));
                }

                foreach (Twig<TInput, TOutput> twig in branch.Value.Dependencies)
                {
                    if (twig.Type.IsAbstract || !twig.Type.GetInterfaces().Contains(typeof(IResponder)))
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

            return errors.Count != 0 ? Result.Fail(errors) : Result.Ok();
        }

        public Func<TInput, Task<Result<TOutput>>> CompileTreeDelegate(IServiceProvider serviceProvider)
        {
            // Mark branches as dependencies if they are referenced by other branches
            foreach (Twig<TInput, TOutput> branch in _dependencies.Values)
            {
                foreach (Twig<TInput, TOutput> twig in branch.Dependencies)
                {
                    twig.IsDependancy = true;
                }

                CompileBranchDelegate(branch, serviceProvider);
            }

            // Select all branches that are not dependencies
            Dictionary<Twig<TInput, TOutput>, Func<TInput, Task<Result<TOutput>>>> branchDelegates = _dependencies
                .Where(branch => !branch.Value.IsDependancy)
                .ToDictionary(branch => branch.Value, branch => branch.Value.CompiledDelegate!);

            return async context =>
            {
                Twig<TInput, TOutput> branch = null!;
                List<IError> errors = new();
                try
                {
                    foreach ((Twig<TInput, TOutput> twig, Func<TInput, Task<Result<TOutput>>> branchDelegate) in branchDelegates)
                    {
                        branch = twig;
                        Result<TOutput> result = await branchDelegate(context);
                        if (result.IsFailed)
                        {
                            errors.AddRange(result.Errors);
                        }
                        else if (result.Value != default)
                        {
                            return result;
                        }
                    }
                }
                catch (Exception error)
                {
                    return Result.Fail(new ResponderExecutionFailedError<TInput, TOutput>(branch, error));
                }

                return Result.Fail(new ResponderExecutionFailedError<TInput, TOutput>().CausedBy(errors));
            };
        }

        private Func<TInput, Task<Result<TOutput>>> CompileBranchDelegate(Twig<TInput, TOutput> branch, IServiceProvider serviceProvider)
        {
            if (_compiledDelegates.TryGetValue(branch, out Func<TInput, Task<Result<TOutput>>>? compiledDelegate))
            {
                return compiledDelegate;
            }

            List<Func<TInput, Task<Result<TOutput>>>> twigDelegates = new();
            foreach (Twig<TInput, TOutput> twig in branch.Dependencies)
            {
                twigDelegates.Add(CompileBranchDelegate(twig, serviceProvider));
            }

            // Add the branch delegate last so that it's only called when all other twigs succeed.
            twigDelegates.Add(ActivatorUtilities.CreateInstance<IResponder<TInput, TOutput>>(serviceProvider, branch.Type).RespondAsync);

            async Task<Result<TOutput>> branchDelegate(TInput context)
            {
                List<IError> errors = new();
                foreach (Func<TInput, Task<Result<TOutput>>> twigDelegate in twigDelegates)
                {
                    Result<TOutput> result = await twigDelegate(context);
                    if (result.IsFailed)
                    {
                        errors.AddRange(result.Errors);
                    }
                    else if (result.Value != default)
                    {
                        return result;
                    }
                }

                return Result.Fail(errors);
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

            foreach (Twig<TInput, TOutput> dependency in _dependencies[twig].Dependencies)
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
