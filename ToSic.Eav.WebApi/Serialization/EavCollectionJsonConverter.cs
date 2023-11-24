using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToSic.Eav.Data;
using ToSic.Eav.DataFormats.EavLight;

namespace ToSic.Eav.WebApi.Serialization;

public class EavCollectionJsonConverter : JsonConverter<IEnumerable<IEntity>>
{
    private readonly IConvertToEavLight _convertToEavLight;

    public EavCollectionJsonConverter(IConvertToEavLight convertToEavLight)
    {
        _convertToEavLight = convertToEavLight;
    }

    public override IEnumerable<IEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, IEnumerable<IEntity> entity, JsonSerializerOptions options)
    {
        var eavLightEntity = _convertToEavLight.Convert(entity);
        JsonSerializer.Serialize(writer, eavLightEntity, eavLightEntity.GetType(), options);
    }
}