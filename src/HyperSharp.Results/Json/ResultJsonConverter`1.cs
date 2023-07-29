using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OoLunar.HyperSharp.Results.Json
{
    public sealed class ResultJsonConverter<T> : JsonConverter<Result<T>>
    {
        public override Result<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, Result<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.Value, options);
            writer.WritePropertyName("errors");
            JsonSerializer.Serialize(writer, value.Errors, options);
            writer.WriteEndObject();
        }
    }
}
