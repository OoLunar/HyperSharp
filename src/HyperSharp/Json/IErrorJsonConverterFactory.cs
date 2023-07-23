using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;

namespace OoLunar.HyperSharp.Json
{
    public sealed class IErrorJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeof(IError).IsAssignableFrom(typeToConvert);
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter?)Activator.CreateInstance(typeof(IErrorJsonConverter<>).MakeGenericType(typeToConvert));
    }
}
