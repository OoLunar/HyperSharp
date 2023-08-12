namespace HyperSharp.Generators;

using System.Collections.Generic;

// we'd love to make this a record struct, or at least a readonly struct with init accessors, but... netstandard.
internal struct ConstructorModel
{
    public bool SkipGeneration { get; set; }

    public string EnclosingType { get; set; }

    public string EnclosingTypeKeyword { get; set; }

    public string EnclosingNamespace { get; set; }

    public IDictionary<string, string>? Parameters { get; set; }
}
