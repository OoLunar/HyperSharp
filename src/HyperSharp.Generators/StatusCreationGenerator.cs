using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HyperSharp.Generators;

/// <summary>
/// Generates static creation methods for statuses
/// </summary>
[Generator]
public class StatusCreationGenerator : IIncrementalGenerator
{
    public void Initialize
    (
        IncrementalGeneratorInitializationContext context
    )
    {
        context.RegisterPostInitializationOutput
        (
            ctx => ctx.AddSource
            (
                "GenerateStatusCreationAttribute.g.cs",
                Constants.Attribute
            )
        );

        IncrementalValuesProvider<ConstructorModel> constructors = context.SyntaxProvider.ForAttributeWithMetadataName<ConstructorModel>
        (
            fullyQualifiedMetadataName: "HyperSharp.Generators.GenerateStatusCreationAttribute",
            predicate: (node, _) =>
            {
                if (node is not ConstructorDeclarationSyntax ctor)
                {
                    return false;
                }

                // ensure there is at least one parameter
                if (ctor.ParameterList.ChildNodes().FirstOrDefault() is not ParameterSyntax parameter)
                {
                    return false;
                }

                // make sure we're dealing with a HttpStatusCode.
                // EndsWith because we want to catch the fully qualified name too, even if this technically can be
                // tricked by a type like PrefixHttpStatusCode.
                // this is the FAWMN predicate, though, so we don't want to conduct too extensive testing because
                // every pointless instruction here directly hurts build/IDE performance
                return parameter.ChildNodes().FirstOrDefault() is IdentifierNameSyntax type
                    && type.Identifier.Text.EndsWith("HttpStatusCode");
            },
            transform: (ctx, _) =>
            {
                IMethodSymbol symbol = (IMethodSymbol)ctx.TargetSymbol;

                // ideally, we want to ship an analyzer with the generator that flags annotated methods that
                // don't fit this criterion.
                if (symbol.Parameters[0].Type.GetFullyQualifiedName() != "global::System.Net.HttpStatusCode")
                {
                    return new ConstructorModel
                    {
                        SkipGeneration = true
                    };
                }

                Dictionary<string, string> parameters = new();

                foreach (IParameterSymbol parameter in symbol.Parameters)
                {
                    if (parameter.Type.GetFullyQualifiedName() != "global::System.Net.HttpStatusCode")
                    {
                        parameters.Add(parameter.Name, parameter.Type.GetFullyQualifiedName());
                    }
                }

                string keyword = symbol.ContainingType.IsValueType
                    ? symbol.ContainingType.IsRecord ? "record struct" : "struct"
                    : symbol.ContainingType.IsRecord ? "record" : "class";

                return new ConstructorModel
                {
                    SkipGeneration = false,
                    EnclosingNamespace = symbol.ContainingType.ContainingNamespace.GetFullNamespace(),
                    EnclosingType = symbol.ContainingType.Name,
                    EnclosingTypeKeyword = keyword,
                    Parameters = parameters
                };
            }
        );

        // here we can theoretically do more processing. avoid capturing Compilation, SemanticModel, AST-related
        // types or symbols in this part of the pipeline and keep them to the initial transform as much as possible.
        // any capturing of these breaks incrementality and thus the reason to use IIncrementalGenerator in the
        // first place and will be a major IDE/intellisense performance issue.

        context.RegisterImplementationSourceOutput<ConstructorModel>
        (
            source: constructors,
            action: (ctx, model) => ctx.AddSource
                (
                    // normally you want your names to be deterministic here
                    $"{model.EnclosingType}-{Guid.NewGuid()}.g.cs",
                    StatusCreationEmitter.Emit(model)
                )
        );
    }
}
