using System.Text.Json.Serialization;
using System.Text.Json;

namespace DoubleSidedDoors.Utils;

public class LocaleTextConverter : JsonConverter<LocaleText>
{
    public override bool HandleNull => true;

    public override LocaleText Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
#pragma warning disable CS8604
        return reader.TokenType switch
        {
            JsonTokenType.String => new LocaleText(reader.GetString()),
            JsonTokenType.Number => new LocaleText(reader.GetUInt32()),
            JsonTokenType.Null => LocaleText.Empty,
            _ => throw new JsonException($"[LocaleTextJson] Type: {reader.TokenType} is not implemented!"),
        };
#pragma warning restore CS8604
    }

    public override void Write(Utf8JsonWriter writer, LocaleText value, JsonSerializerOptions options)
    {
        if (value.ID != 0)
        {
            writer.WriteNumberValue(value.ID);
        }
        else
        {
            writer.WriteStringValue(value.RawText);
        }
    }
}
