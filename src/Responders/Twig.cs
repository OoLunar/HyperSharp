using System;
using System.Threading.Tasks;
using FluentResults;

namespace OoLunar.HyperSharp.Responders
{
    public sealed class Twig
    {
        public Type Type { get; init; }
        public Twig[] Dependencies { get; init; }
        public bool IsDependancy { get; internal set; }
        public Func<HyperContext, Task<Result<HyperStatus>>>? CompiledDelegate { get; internal set; }

        public Twig(Type type, Twig[] dependencies)
        {
            Type = type;
            Dependencies = dependencies;
        }
    }
}
