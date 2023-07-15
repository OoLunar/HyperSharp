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
        private readonly Dictionary<Type, Type[]> _dependencies = new();

        public void RegisterResponders(Assembly assembly) => RegisterResponders(assembly.GetTypes());
        public void RegisterResponders(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                if (type.IsAbstract || !type.GetInterfaces().Contains(typeof(IResponder)))
                {
                    continue;
                }

                List<Type> implements = new() { type };
                IEnumerable<Type> dependsOnAttributes = type
                    .GetCustomAttributes<DependsOnAttribute>(inherit: true)
                    .SelectMany(attribute => attribute.Dependencies);

                foreach (Type dependsOnType in dependsOnAttributes)
                {
                    implements.Add(dependsOnType);
                }

                _dependencies[type] = implements.Distinct().ToArray();
            }
        }

        public Result ValidateResponders()
        {
            List<IError> errors = new();
            foreach (KeyValuePair<Type, Type[]> branch in _dependencies)
            {
                if (branch.Key.IsAbstract || !branch.Key.GetInterfaces().Contains(typeof(IResponder)))
                {
                    errors.Add(new Error($"Invalid dependency: Responder {branch.Key.Name} is not a responder."));
                }

                foreach (Type twig in branch.Value)
                {
                    // TODO: Create own error types which inherit from IError.
                    if (twig.IsAbstract || !twig.GetInterfaces().Contains(typeof(IResponder)))
                    {
                        errors.Add(new Error($"Invalid dependency: Responder {branch.Key.Name} depends on {twig.Name}, which is not a responder."));
                    }
                    else if (!_dependencies.TryGetValue(twig, out Type[]? twigs))
                    {
                        errors.Add(new Error($"Missing dependency: Responder {branch.Key.Name} depends on {twig.Name}, which is not registered."));
                    }
                    else if (twigs.Contains(branch.Key))
                    {
                        errors.Add(new Error($"Recursive dependency detected: Responder {branch.Key.Name} depends on {twig.Name}, which depends on {branch.Key.Name}."));
                    }
                }
            }

            return Result.Ok();
        }

        public Func<HyperContext, Task<Result<HyperStatus>>> CompileTreeDelegate(IServiceProvider serviceProvider)
        {
            List<Func<HyperContext, Task<Result<HyperStatus>>>> branchDelegates = new();
            foreach (KeyValuePair<Type, Type[]> branch in _dependencies)
            {
                branchDelegates.Add(CompileBranchDelegate(branch.Key, branch.Value, serviceProvider));
            }

            return async context =>
            {
                List<IError> errors = new();
                foreach (Func<HyperContext, Task<Result<HyperStatus>>> branchDelegate in branchDelegates)
                {
                    Result<HyperStatus> result = await branchDelegate(context);
                    if (result.IsSuccess)
                    {
                        return result;
                    }

                    errors.AddRange(result.Errors);
                }

                return Result.Fail(new Error("No responder succeeded.").CausedBy(errors));
            };
        }

        public static Func<HyperContext, Task<Result<HyperStatus>>> CompileBranchDelegate(Type branch, Type[] twigs, IServiceProvider serviceProvider)
        {
            List<Func<HyperContext, Task<Result<HyperStatus>>>> twigDelegates = new();
            foreach (Type twig in twigs)
            {
                twigDelegates.Add(((IResponder)ActivatorUtilities.CreateInstance(serviceProvider, twig)).RespondAsync);
            }

            // Add the branch delegate last so that it's only called when all other twigs succeed.
            twigDelegates.Add(((IResponder)ActivatorUtilities.CreateInstance(serviceProvider, branch)).RespondAsync);

            return async context =>
            {
                List<IError> errors = new();
                foreach (Func<HyperContext, Task<Result<HyperStatus>>> twigDelegate in twigDelegates)
                {
                    Result<HyperStatus> result = await twigDelegate(context);
                    if (result.IsSuccess)
                    {
                        return result;
                    }
                    errors.AddRange(result.Errors);
                }

                return Result.Fail(errors);
            };
        }
    }
}
