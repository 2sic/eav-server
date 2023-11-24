using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToSic.Eav.Data;
using ToSic.Eav.DataFormats.EavLight;

namespace ToSic.Eav.WebApi.Serialization;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EavJsonConverter : JsonConverter<IEntity>
{
    private readonly IConvertToEavLight _convertToEavLight;

    public EavJsonConverter(IConvertToEavLight convertToEavLight)
    {
        _convertToEavLight = convertToEavLight;
    }

    public override IEntity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, IEntity entity, JsonSerializerOptions options)
    {
        var eavLightEntity = _convertToEavLight.Convert(entity);
        JsonSerializer.Serialize(writer, eavLightEntity, eavLightEntity.GetType(), options);
    }
}