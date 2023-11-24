using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToSic.Eav.Serialization;

public class JsonDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDateTime().ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // fix: 2 hours are subtracted from the time on each load
        // less decimals after seconds are to preserve compatibility with older api that used Newtonsoft
        writer.WriteStringValue(value/*.ToUniversalTime()*/.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}