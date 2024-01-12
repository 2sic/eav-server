using System.Text.Json;
using ToSic.Eav.DataFormats.EavLight;

namespace ToSic.Eav.WebApi.Serialization;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EavJsonConverter(IConvertToEavLight convertToEavLight) : JsonConverter<IEntity>
{
    public override IEntity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, IEntity entity, JsonSerializerOptions options)
    {
        var eavLightEntity = convertToEavLight.Convert(entity);
        JsonSerializer.Serialize(writer, eavLightEntity, eavLightEntity.GetType(), options);
    }
}