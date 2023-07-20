using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.HyperSharp.Responders;

namespace OoLunar.HyperSharp
{
    public class ResponderSearcher
    {
        private readonly Dictionary<Type, Twig> _dependencies = new();
        private readonly Dictionary<Twig, Func<HyperContext, Task<Result<HyperStatus>>>> _compiledDelegates = new();

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

            List<Twig> twigDependencies = new();
            foreach (Type? dependencyType in dependsOnAttributes)
            {
                if (!_dependencies.ContainsKey(dependencyType))
                {
                    RegisterResponder(dependencyType); // Register dependent twigs recursively
                }

                twigDependencies.Add(_dependencies[dependencyType]);
            }

            Twig twig = new(type, twigDependencies.ToArray());
            _dependencies[type] = twig;
        }

        public Result ValidateResponders()
        {
            List<IError> errors = new();
            foreach (KeyValuePair<Type, Twig> branch in _dependencies)
            {
                if (branch.Key.IsAbstract || !branch.Key.GetInterfaces().Contains(typeof(IResponder)))
                {
                    errors.Add(new Error($"Invalid dependency: Responder {branch.Key.Name} is not a responder."));
                }

                foreach (Twig twig in branch.Value.Dependencies)
                {
                    // TODO: Create own error types which inherit from IError.
                    if (twig.Type.IsAbstract || !twig.Type.GetInterfaces().Contains(typeof(IResponder)))
                    {
                        errors.Add(new Error($"Invalid dependency: Responder {branch.Key.Name} depends on {twig.Type.Name}, which is not a responder."));
                    }
                    else if (!_dependencies.ContainsKey(twig.Type))
                    {
                        errors.Add(new Error($"Missing dependency: Responder {branch.Key.Name} depends on {twig.Type.Name}, which is not registered."));
                    }
                    else if (CheckRecursiveDependency(branch.Key, twig.Type))
                    {
                        errors.Add(new Error($"Recursive dependency detected: Responder {branch.Key.Name} depends on {twig.Type.Name}, which depends on {branch.Key.Name}."));
                    }
                }
            }

            return errors.Count != 0 ? Result.Fail(errors) : Result.Ok();
        }

        public Func<HyperContext, Task<Result<HyperStatus>>> CompileTreeDelegate(IServiceProvider serviceProvider)
        {
            // Mark branches as dependencies if they are referenced by other branches
            foreach (Twig branch in _dependencies.Values)
            {
                foreach (Twig twig in branch.Dependencies)
                {
                    twig.IsDependancy = true;
                }

                CompileBranchDelegate(branch, serviceProvider);
            }

            List<Func<HyperContext, Task<Result<HyperStatus>>>> branchDelegates = new();
            foreach (Twig branch in _dependencies.Values)
            {
                if (!branch.IsDependancy)
                {
                    branchDelegates.Add(_compiledDelegates[branch]);
                }
            }

            return async context =>
            {
                List<IError> errors = new();
                try
                {
                    foreach (Func<HyperContext, Task<Result<HyperStatus>>> branchDelegate in branchDelegates)
                    {
                        Result<HyperStatus> result = await branchDelegate(context);
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
                    errors.Add(new Error(error.Message).CausedBy(error));
                }

                return Result.Fail(new Error("No responder succeeded.").CausedBy(errors));
            };
        }

        private Func<HyperContext, Task<Result<HyperStatus>>> CompileBranchDelegate(Twig branch, IServiceProvider serviceProvider)
        {
            if (_compiledDelegates.TryGetValue(branch, out Func<HyperContext, Task<Result<HyperStatus>>>? compiledDelegate))
            {
                return compiledDelegate;
            }

            List<Func<HyperContext, Task<Result<HyperStatus>>>> twigDelegates = new();
            foreach (Twig twig in branch.Dependencies)
            {
                twigDelegates.Add(CompileBranchDelegate(twig, serviceProvider));
            }

            // Add the branch delegate last so that it's only called when all other twigs succeed.
            twigDelegates.Add(((IResponder)ActivatorUtilities.CreateInstance(serviceProvider, branch.Type)).RespondAsync);

            async Task<Result<HyperStatus>> branchDelegate(HyperContext context)
            {
                List<IError> errors = new();
                foreach (Func<HyperContext, Task<Result<HyperStatus>>> twigDelegate in twigDelegates)
                {
                    Result<HyperStatus> result = await twigDelegate(context);
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

            _compiledDelegates[branch] = branchDelegate;
            return branchDelegate;
        }

        private bool CheckRecursiveDependency(Type branch, Type twig)
        {
            if (twig == branch)
            {
                return true;
            }

            foreach (Twig dependency in _dependencies[twig].Dependencies)
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
