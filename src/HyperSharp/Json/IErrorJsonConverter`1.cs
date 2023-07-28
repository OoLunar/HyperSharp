using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Json
{
    public sealed class IErrorJsonConverter<T> : JsonConverter<T> where T : Error
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("message", value.Message);
            if (value.Errors.Any())
            {
                writer.WriteStartArray("reasons");
                foreach (Error error in value.Errors)
                {
                    Write(writer, (T)error, options);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
    }
}
