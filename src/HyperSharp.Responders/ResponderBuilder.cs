using System;
using System.Collections.Generic;

namespace OoLunar.HyperSharp.Responders
{
    internal sealed record ResponderBuilder
    {
        public Type Type { get; init; }
        public bool IsSyncronous { get; init; }
        public List<Type> Dependencies { get; init; }
        public List<Type> RequiredBy { get; init; }
        public string Name => Type.FullName ?? "<runtime generated type>";

        public ResponderBuilder(Type type, bool isSyncronous, IEnumerable<Type> dependencies)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            ArgumentNullException.ThrowIfNull(dependencies, nameof(dependencies));

            Type = type;
            IsSyncronous = isSyncronous;
            Dependencies = new(dependencies);
            RequiredBy = new();
        }
    }
}
