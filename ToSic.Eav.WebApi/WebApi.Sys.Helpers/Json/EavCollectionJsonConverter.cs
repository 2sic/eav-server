using System.Text.Json;
using ToSic.Eav.DataFormats.EavLight;

namespace ToSic.Eav.WebApi.Sys.Helpers.Json;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EavCollectionJsonConverter(IConvertToEavLight convertToEavLight) : JsonConverter<IEnumerable<IEntity>>
{
    public override IEnumerable<IEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, IEnumerable<IEntity> entity, JsonSerializerOptions options)
    {
        var eavLightEntity = convertToEavLight.Convert(entity);
        JsonSerializer.Serialize(writer, eavLightEntity, eavLightEntity.GetType(), options);
    }
}