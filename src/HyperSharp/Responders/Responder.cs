using System;

namespace OoLunar.HyperSharp.Responders
{
    public sealed class Responder<TInput, TOutput> where TOutput : new()
    {
        public Type Type { get; init; }
        public bool IsDependancy { get; internal set; }
        public Responder<TInput, TOutput>[] Dependencies { get; init; }
        public ResponderDelegate<TInput, TOutput>? CompiledDelegate { get; internal set; }

        public Responder(Type type, Responder<TInput, TOutput>[] dependencies)
        {
            Type = type;
            Dependencies = dependencies;
        }
    }
}
