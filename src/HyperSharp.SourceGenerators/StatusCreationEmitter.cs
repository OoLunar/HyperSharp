using System.Collections.Generic;
using System.Text;

namespace HyperSharp.SourceGenerators;

internal static class StatusCreationEmitter
{
    public static string Emit(ConstructorModel model)
    {
        if (model.SkipGeneration)
        {
            return "";
        }

        StringBuilder builder = new();
        builder.Append(
$$"""
// <auto-generated/>
namespace {{model.EnclosingNamespace}}
{
    partial {{model.EnclosingTypeKeyword}} {{model.EnclosingType}}
    {
""");
        // skip processing parameters if we're dealing with zero additional parameters
        if (model.Parameters!.Count == 0)
        {
            builder.Append('\n');

            foreach (string code in Constants.HttpStatuses)
            {
                builder.Append(
$$"""
        /// <summary>
        /// Creates a new instance with the status code <c>HttpStatusCode.{{code}}</c>.
        /// </summary>
        public static {{model.EnclosingType}} {{code}}() => new(global::System.Net.HttpStatusCode.{{code}});
""");
            }

            builder.Append(
$$"""
    }
}
""");

            return builder.ToString();
        }

        StringBuilder parameterListBuilder = new();
        foreach (KeyValuePair<string, string> parameter in model.Parameters!)
        {
            parameterListBuilder.Append($"{parameter.Value} {parameter.Key}, ");
        }

        // remove the last comma and space
        parameterListBuilder.Remove(parameterListBuilder.Length - 2, 2);
        string parameterList = parameterListBuilder.ToString();

        StringBuilder parameterNameListBuilder = new();
        foreach (KeyValuePair<string, string> parameter in model.Parameters)
        {
            parameterNameListBuilder.Append($"{parameter.Key}, ");
        }

        // remove the last comma and space
        parameterNameListBuilder.Remove(parameterNameListBuilder.Length - 2, 2);
        string parameterNameList = parameterNameListBuilder.ToString();
        foreach (string code in Constants.HttpStatuses)
        {
            builder.Append(
$$"""
    /// <summary>
    /// Creates a new instance with the status code <c>HttpStatusCode.{{code}}</c>.
    /// </summary>
    public static {{model.EnclosingType}} {{code}}({{parameterList}}) => new(global::System.Net.HttpStatusCode.{{code}}, {{parameterNameList}});
""");
        }

        builder.Append(
$$"""
    }
}
""");

        return builder.ToString();
    }
}