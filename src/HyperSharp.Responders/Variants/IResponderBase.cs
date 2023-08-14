using System;
using System.Reflection;

namespace HyperSharp.Responders
{
    /// <summary>
    /// Represents a responder.
    /// </summary>
    public interface IResponderBase
    {
        /// <summary>
        /// The types that the responder depends on.
        /// </summary>
        static abstract Type[] Needs { get; }

        internal static (Type[] Needs, bool IsSyncronous) RetrieveResponderMetadata(Type type) => (
            Needs: (Type[])type.GetProperty(nameof(Needs), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)!.GetValue(null)!,
            IsSyncronous: typeof(IResponder).IsAssignableFrom(type)
        );
    }
}
