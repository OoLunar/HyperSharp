using System;
using System.Threading.Tasks;
using FluentResults;

namespace OoLunar.HyperSharp.Responders
{
    public sealed class Twig<TInput, TResult>
    {
        public Type Type { get; init; }
        public Twig<TInput, TResult>[] Dependencies { get; init; }
        public bool IsDependancy { get; internal set; }
        public Func<TInput, Task<Result<TResult>>>? CompiledDelegate { get; internal set; }

        public Twig(Type type, Twig<TInput, TResult>[] dependencies)
        {
            Type = type;
            Dependencies = dependencies;
        }
    }
}
