using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HyperSharp.SourceGenerators;

/// <summary>
/// Generates static creation methods for statuses
/// </summary>
[Generator]
public class StatusCreationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("GenerateStatusCreationAttribute.g.cs", Constants.Attribute));
        IncrementalValuesProvider<ConstructorModel> constructors = context.SyntaxProvider.ForAttributeWithMetadataName<ConstructorModel>(
            fullyQualifiedMetadataName: "HyperSharp.SourceGenerators.GenerateStatusCreationAttribute",
            predicate: Predicate,
            transform: Transform
        );

        // Here we can theoretically do more processing. avoid capturing Compilation, SemanticModel, AST-related types or symbols in this part of the pipeline and keep them to the initial transform as much as possible.
        // Any capturing of these breaks incrementality and thus the reason to use IIncrementalGenerator in the first place and will be a major IDE/intellisense performance issue.
        context.RegisterImplementationSourceOutput<ConstructorModel>
        (
            source: constructors,
            action: (ctx, model) => ctx.AddSource($"{model.EnclosingType}.g.cs", StatusCreationEmitter.Emit(model))
        );
    }

    private static bool Predicate(SyntaxNode node, CancellationToken cancellationToken = default)
    {
        // Ensure there is at least one parameter
        if (node is not ConstructorDeclarationSyntax ctor || ctor.ParameterList.ChildNodes().FirstOrDefault() is not ParameterSyntax parameter)
        {
            return false;
        }

        // Make sure we're dealing with a HttpStatusCode.
        // EndsWith because we want to catch the fully qualified name too, even if this technically can be tricked by a type like PrefixHttpStatusCode.
        // This is the FAWMN predicate, though, so we don't want to conduct too extensive testing because every pointless instruction here directly hurts build/IDE performance
        return parameter.ChildNodes().FirstOrDefault() is IdentifierNameSyntax type && type.Identifier.Text.EndsWith("HttpStatusCode");
    }

    private static ConstructorModel Transform(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken = default)
    {
        IMethodSymbol symbol = (IMethodSymbol)ctx.TargetSymbol;

        // ideally, we want to ship an analyzer with the generator that flags annotated methods that don't fit this criterion.
        if (GetFullyQualifiedName(symbol.Parameters[0].Type) != "global::System.Net.HttpStatusCode")
        {
            return new ConstructorModel(
                skipGeneration: true,
                enclosingNamespace: "",
                enclosingType: "",
                enclosingTypeKeyword: "",
                parameters: null
            );
        }

        Dictionary<string, string> parameters = new();
        foreach (IParameterSymbol parameter in symbol.Parameters)
        {
            string fullyParameterQualifiedName = GetFullyQualifiedName(parameter.Type);
            if (fullyParameterQualifiedName != "global::System.Net.HttpStatusCode")
            {
                parameters.Add(parameter.Name, fullyParameterQualifiedName);
            }
        }

        string keyword = (symbol.ContainingType.IsValueType, symbol.ContainingType.IsRecord) switch
        {
            (true, true) => "record struct",
            (false, true) => "record",
            (true, false) => "struct",
            (false, false) => "class",
        };

        return new ConstructorModel(
            skipGeneration: false,
            enclosingNamespace: GetFullNamespace(symbol.ContainingType.ContainingNamespace),
            enclosingType: symbol.ContainingType.Name,
            enclosingTypeKeyword: keyword,
            parameters: parameters
        );
    }

    public static string GetFullyQualifiedName(ITypeSymbol symbol) => $"global::{GetFullNamespace(symbol.ContainingNamespace)}.{symbol.Name}";
    public static string GetFullNamespace(INamespaceSymbol symbol)
    {
        INamespaceSymbol fullNamespace = symbol;
        List<string> names = new() { fullNamespace.Name };

        while (!fullNamespace.ContainingNamespace.IsGlobalNamespace)
        {
            fullNamespace = fullNamespace.ContainingNamespace;
            names.Add(fullNamespace.Name);
        }

        names.Reverse();
        return string.Join(".", names);
    }
}
