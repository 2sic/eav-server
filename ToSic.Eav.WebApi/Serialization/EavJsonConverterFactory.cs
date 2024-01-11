using System.Collections;
using System.Text.Json;

namespace ToSic.Eav.WebApi.Serialization;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EavJsonConverterFactory : JsonConverterFactory
{
    private readonly EavJsonConverter _eavJsonConverter;
    private readonly EavCollectionJsonConverter _eavCollectionJsonConverter;

    public EavJsonConverterFactory(EavJsonConverter eavJsonConverter, EavCollectionJsonConverter eavCollectionJsonConverter)
    {
        _eavJsonConverter = eavJsonConverter;
        _eavCollectionJsonConverter = eavCollectionJsonConverter;
    }
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IEntity).IsAssignableFrom(typeToConvert)
               || typeof(IEnumerable<IEntity>).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return typeof(IEnumerable).IsAssignableFrom(typeToConvert)
            ? (JsonConverter) _eavCollectionJsonConverter
            : _eavJsonConverter;
    }
}