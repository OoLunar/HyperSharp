using Microsoft.CodeAnalysis;

namespace HyperSharp.SourceGenerators;

internal static class TypeSymbolExtensions
{
    public static string GetFullyQualifiedName(this ITypeSymbol symbol)
        => $"global::{symbol.ContainingNamespace.GetFullNamespace()}.{symbol.Name}";

    public static string GetFullyQualifiedName(this INamedTypeSymbol symbol)
        => $"global::{symbol.ContainingNamespace.GetFullNamespace()}.{symbol.Name}";
}
