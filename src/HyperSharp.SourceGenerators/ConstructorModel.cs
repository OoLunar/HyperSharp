namespace HyperSharp.SourceGenerators;

using System.Collections.Generic;

internal readonly record struct ConstructorModel
{
    public bool SkipGeneration { get; }
    public string EnclosingType { get; }
    public string EnclosingTypeKeyword { get; }
    public string EnclosingNamespace { get; }
    public IDictionary<string, string>? Parameters { get; }

    public ConstructorModel(
        bool skipGeneration,
        string enclosingType,
        string enclosingTypeKeyword,
        string enclosingNamespace,
        IDictionary<string, string>? parameters = null)
    {
        SkipGeneration = skipGeneration;
        EnclosingType = enclosingType;
        EnclosingTypeKeyword = enclosingTypeKeyword;
        EnclosingNamespace = enclosingNamespace;
        Parameters = parameters;
    }
}
