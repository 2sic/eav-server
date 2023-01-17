using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToSic.Eav.Data;
using ToSic.Eav.DataFormats.EavLight;

namespace ToSic.Eav.WebApi.Serialization
{
    public class EntityWrapperCollectionJsonConverter : JsonConverter<IEnumerable<IEntityWrapper>>
    {
        private readonly IConvertToEavLight _convertToEavLight;

        public EntityWrapperCollectionJsonConverter(IConvertToEavLight convertToEavLight)
        {
            _convertToEavLight = convertToEavLight;
        }

        public override IEnumerable<IEntityWrapper> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
            => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, IEnumerable<IEntityWrapper> entities, JsonSerializerOptions options)
        {
            var eavLightEntity = _convertToEavLight.Convert(entities);
            JsonSerializer.Serialize(writer, eavLightEntity, eavLightEntity.GetType(), options);
        }
    }
}
