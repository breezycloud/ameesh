using System;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Shared.Helpers;

public class NullableIntConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && string.IsNullOrEmpty(reader.GetString()))
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32();
        }

        throw new JsonException("Invalid token type for int?");
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

