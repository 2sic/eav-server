using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToSic.Eav.Data;
using ToSic.Eav.DataFormats.EavLight;

namespace ToSic.Eav.WebApi.Serialization
{
    public class EntityWrapperJsonConverter : JsonConverter<IEntityWrapper>
    {
        private readonly IConvertToEavLight _convertToEavLight;

        public EntityWrapperJsonConverter(IConvertToEavLight convertToEavLight)
        {
            _convertToEavLight = convertToEavLight;
        }

        public override IEntityWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
            => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, IEntityWrapper entity, JsonSerializerOptions options)
        {
            var eavLightEntity = _convertToEavLight.Convert(entity);
            JsonSerializer.Serialize(writer, eavLightEntity, eavLightEntity.GetType(), options);
        }
    }
}
