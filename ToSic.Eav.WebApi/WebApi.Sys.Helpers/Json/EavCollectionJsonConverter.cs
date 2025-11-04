using System.Text.Json;
using ToSic.Eav.DataFormats.EavLight;

namespace ToSic.Eav.WebApi.Sys.Helpers.Json;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EavCollectionJsonConverter(IConvertToEavLight convertToEavLight) : JsonConverter<IEnumerable<IEntity>>
{
    public override IEnumerable<IEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, IEnumerable<IEntity> entities, JsonSerializerOptions options)
    {
        var eavLightEntities = convertToEavLight.Convert(entities);
        JsonSerializer.Serialize(writer, eavLightEntities, eavLightEntities.GetType(), options);
    }
}