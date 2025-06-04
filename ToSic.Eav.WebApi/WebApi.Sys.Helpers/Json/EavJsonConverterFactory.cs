using System.Collections;
using System.Text.Json;

namespace ToSic.Eav.WebApi.Sys.Helpers.Json;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EavJsonConverterFactory(
    EavJsonConverter eavJsonConverter,
    EavCollectionJsonConverter eavCollectionJsonConverter)
    : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(IEntity).IsAssignableFrom(typeToConvert)
        || typeof(IEnumerable<IEntity>).IsAssignableFrom(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        typeof(IEnumerable).IsAssignableFrom(typeToConvert)
            ? eavCollectionJsonConverter
            : eavJsonConverter;
}