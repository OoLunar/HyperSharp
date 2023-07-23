using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;

namespace OoLunar.HyperSharp.Json
{
    public sealed class IErrorJsonConverter<T> : JsonConverter<T> where T : IError
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("message", value.Message);
            if (value.Reasons.Count > 0)
            {
                writer.WriteStartArray("reasons");
                foreach (IError error in value.Reasons)
                {
                    Write(writer, (T)error, options);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
    }
}
