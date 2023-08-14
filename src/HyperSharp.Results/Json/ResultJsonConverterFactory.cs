using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HyperSharp.Results.Json
{
    /// <summary>
    /// Creates JSON converters for <see cref="Result{T}"/>.
    /// </summary>
    public sealed class ResultJsonConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);

        /// <inheritdoc/>
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter?)Activator.CreateInstance(typeof(ResultJsonConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]));
    }
}
