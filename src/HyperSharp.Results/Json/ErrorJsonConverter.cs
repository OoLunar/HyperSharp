using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HyperSharp.Results.Json
{
    /// <summary>
    /// Converts an <see cref="Error"/> to and from JSON.
    /// </summary>
    public sealed class ErrorJsonConverter : JsonConverter<Error>
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Error);

        /// <inheritdoc/>
        public override Error Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected the start of an object.");
            }

            string? message = null;
            Error[]? errors = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }
                else if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected a property name.");
                }

                string propertyName = reader.GetString()!;
                if (!reader.Read())
                {
                    throw new JsonException("Expected a value after the property name.");
                }

                switch (propertyName)
                {
                    case "message":
                        message = reader.GetString() ?? throw new JsonException("Expected a non-null message value.");
                        break;
                    case "errors":
                        if (reader.TokenType != JsonTokenType.StartArray)
                        {
                            throw new JsonException("Expected the start of an array for errors.");
                        }

                        errors = JsonSerializer.Deserialize<Error[]>(ref reader, options) ?? throw new JsonException("Failed to deserialize errors array.");
                        break;
                    default:
                        throw new JsonException($"Unexpected property name: {propertyName}");
                }
            }

            if (message is null)
            {
                throw new JsonException("Missing required 'message' property.");
            }
            else if (errors is null)
            {
                return new Error(message);
            }

            return new Error(message, errors);
        }

        /// <inheritdoc/>
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
