using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HyperSharp.Results.Json
{
    /// <summary>
    /// Converts a <see cref="Result"/> to and from JSON.
    /// </summary>
    public sealed class ResultJsonConverter : JsonConverter<Result>
    {
        /// <inheritdoc/>
        public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected the start of an object.");
            }

            object? value = null;
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
                else if (propertyName == "value")
                {
                    value = JsonSerializer.Deserialize(ref reader, typeToConvert, options);
                }
                else if (propertyName == "status")
                {
                    // We don't use the status value, so just skip it.
                    continue;
                }
                else if (propertyName == "errors")
                {
                    if (reader.TokenType != JsonTokenType.StartArray)
                    {
                        throw new JsonException("Expected the start of an array for errors.");
                    }

                    errors = JsonSerializer.Deserialize<Error[]>(ref reader, options);
                }
                else
                {
                    throw new JsonException($"Unexpected property name: {propertyName}");
                }
            }

            return (value, errors) switch
            {
                (null, null) => new Result(),
                (null, object) => new Result(errors),
                (object, null) => new Result(value),
                _ => new Result(value, errors)
            };
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.Value, options);
            writer.WritePropertyName("status");
            JsonSerializer.Serialize(writer, value.Status, options);
            writer.WritePropertyName("errors");
            JsonSerializer.Serialize(writer, value.Errors, options);
            writer.WriteEndObject();
        }
    }
}
