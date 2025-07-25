﻿using System.Text.Json;

// Based on https://github.com/dotnet/runtime/issues/43026#issuecomment-949966701
namespace ToSic.Eav.Serialization.Sys.Json;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class JsonExceptionConverter<TExceptionType> : JsonConverter<TExceptionType>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Exception).IsAssignableFrom(typeToConvert);
    }

    public override TExceptionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Deserializing exceptions is not allowed");
    }

    public override void Write(Utf8JsonWriter writer, TExceptionType value, JsonSerializerOptions options)
    {
        // Null check safety, should never actually be null
        if (value == null)
            return;

        var serializableProperties = value.GetType()
            .GetProperties()
            .Select(uu => new { uu.Name, Value = uu.GetValue(value) })
            .Where(uu => uu.Name != nameof(Exception.TargetSite));

        if (options.DefaultIgnoreCondition == WhenWritingNull)
        {
            serializableProperties = serializableProperties
                .Where(uu => uu.Value != null);
        }

        var propList = serializableProperties
            .ToListOpt();

        if (propList.Count == 0)
        {
            // Nothing to write
            return;
        }

        writer.WriteStartObject();

        foreach (var prop in propList)
        {
            writer.WritePropertyName(prop.Name);
            JsonSerializer.Serialize(writer, prop.Value, options);
        }

        writer.WriteEndObject();
    }
}