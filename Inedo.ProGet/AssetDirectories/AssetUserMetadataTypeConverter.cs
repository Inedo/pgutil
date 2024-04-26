using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inedo.ProGet.AssetDirectories;

internal sealed class AssetUserMetadataTypeConverter : JsonConverter<AssetUserMetadata>
{
    public override AssetUserMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new AssetUserMetadata(reader.GetString()!, false);

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        string? value = null;
        bool includeInResponseHeader = false;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            if (reader.ValueTextEquals("value"))
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.String)
                    throw new JsonException();

                value = reader.GetString();
            }
            else if (reader.ValueTextEquals("includeInResponseHeader"))
            {
                reader.Read();
                includeInResponseHeader = reader.GetBoolean();
            }
        }

        if (value is null)
            throw new JsonException();

        return new AssetUserMetadata(value, includeInResponseHeader);
    }
    public override void Write(Utf8JsonWriter writer, AssetUserMetadata value, JsonSerializerOptions options)
    {
        if (value.IncludeInResponseHeader)
        {
            writer.WriteStartObject();
            writer.WriteString("value", value.Value);
            writer.WriteBoolean("includeInResponseHeader", true);
            writer.WriteEndObject();
        }
        else
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
