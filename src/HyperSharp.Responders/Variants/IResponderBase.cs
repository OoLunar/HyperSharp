using System;
using System.Reflection;

namespace OoLunar.HyperSharp.Responders
{
    public interface IResponderBase
    {
        static abstract Type[] Needs { get; }

        private static readonly Func<Type, Type[]> _retrieveNeedsProperty;
        private static readonly Func<Type, bool> _isSyncronous;
        static IResponderBase()
        {
            PropertyInfo needsProperty = typeof(IResponderBase).GetProperty(nameof(Needs)) ?? throw new InvalidOperationException($"Could not find \"Needs\" property on {nameof(IResponderBase)}.");
            _retrieveNeedsProperty = (Func<Type, Type[]>)needsProperty.GetGetMethod()!.CreateDelegate(typeof(Func<Type, Type[]>));
            _isSyncronous = typeof(IResponder).IsAssignableFrom;
        }

        internal static (Type[] Needs, bool IsSyncronous) RetrieveResponderMetadata(Type type) => (_retrieveNeedsProperty(type), _isSyncronous(type));
    }
}
