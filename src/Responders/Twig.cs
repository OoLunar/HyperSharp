using System;

namespace OoLunar.HyperSharp.Responders
{
    internal sealed class Twig
    {
        public Type Type { get; init; }
        public Twig[] Dependencies { get; init; }
        public bool IsDependancy { get; internal set; }

        public Twig(Type type, Twig[] dependencies)
        {
            Type = type;
            Dependencies = dependencies;
        }
    }
}
