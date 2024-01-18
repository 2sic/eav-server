using System.Collections;
using System.Text.Json;

namespace ToSic.Eav.WebApi.Serialization;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EavJsonConverterFactory(
    EavJsonConverter eavJsonConverter,
    EavCollectionJsonConverter eavCollectionJsonConverter)
    : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IEntity).IsAssignableFrom(typeToConvert)
               || typeof(IEnumerable<IEntity>).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return typeof(IEnumerable).IsAssignableFrom(typeToConvert)
            ? (JsonConverter) eavCollectionJsonConverter
            : eavJsonConverter;
    }
}