using System.Text.Json;

namespace ToSic.Eav.Serialization;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class JsonShortDateTimeConverter : JsonDateTimeConverter
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}