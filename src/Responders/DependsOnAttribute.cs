using System;
using System.Linq;

namespace OoLunar.HyperSharp.Responders
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = true)]
    public class DependsOnAttribute : Attribute
    {
        public Type[] Dependencies { get; init; }

        public DependsOnAttribute(params Type[] dependencies)
        {
            if (dependencies is null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }
            else if (dependencies.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(dependencies));
            }
            else if (dependencies.Any(dependency => !dependency.GetInterfaces().Contains(typeof(IResponder))))
            {
                throw new ArgumentException("Value must be a collection of IResponder types.", nameof(dependencies));
            }

            Dependencies = dependencies;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = true)]
    public sealed class DependsOnAttribute<T> : DependsOnAttribute where T : IResponder
    {
        public DependsOnAttribute() : base(typeof(T)) { }
    }
}
