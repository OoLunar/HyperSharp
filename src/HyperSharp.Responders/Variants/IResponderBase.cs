using System;
using System.Reflection;

namespace HyperSharp.Responders
{
    public interface IResponderBase
    {
        static abstract Type[] Needs { get; }

        internal static (Type[] Needs, bool IsSyncronous) RetrieveResponderMetadata(Type type) => (
            (Type[])type.GetProperty(nameof(Needs), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)!.GetValue(null)!,
            typeof(IResponder).IsAssignableFrom(type)
        );
    }
}
