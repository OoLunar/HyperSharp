using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HyperSharp.Results.Json
{
    public sealed class ErrorJsonConverter : JsonConverter<Error>
    {
        public override Error? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, Error value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("message", value.Message);
            if (value.Errors.Any())
            {
                writer.WriteStartArray("errors");
                foreach (Error error in value.Errors)
                {
                    Write(writer, error, options);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
    }
}
