using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OoLunar.HyperSharp.Results.Json
{
    public sealed class ResultJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter?)Activator.CreateInstance(typeof(ResultJsonConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]));
    }
}
