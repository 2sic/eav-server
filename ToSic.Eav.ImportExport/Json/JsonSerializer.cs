using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.Xml
{
    public class JsonSerializer: SerializerBase
    {
        public string JsonEntity(int entityId)
        {
            if(App == null)
                throw new Exception($"Can't serialize entity {entityId} without the app package. Please initialize first, or provide a prepared entity");

            return JsonEntity(App.Entities[entityId]);
        }

        public string JsonEntity(IEntity entity)
        {
            var header = new {V = "1.0", TS = DateTime.Now, Content = "Entity"};

            object metadata = null;
            if (entity.Metadata.IsMetadata)
            {
                var mddic = new Dictionary<string, object>()
                {
                    {"Target", App.GetMetadataType(entity.Metadata.TargetType)}
                };
                if(entity.Metadata.KeyGuid != null)
                    mddic.Add("Guid", entity.Metadata.KeyGuid);
                if(entity.Metadata.KeyNumber != null)
                    mddic.Add("Number", entity.Metadata.KeyNumber);
                if(entity.Metadata.KeyString != null)
                    mddic.Add("String", entity.Metadata.KeyString);
                metadata = mddic;
            }

            var serDic = new Dictionary<string, object>
            {
                {"_", header},
                {"id", entity.EntityId},
                {"guid", entity.EntityGuid},
                {"type", new {name = entity.Type.Name, id = entity.Type.StaticName}}
            };
            if (entity.Attributes.Any())
                serDic.Add("attributes", entity.Attributes.Values
                    .OrderBy(a => a.Name)
                    .Where(a => a.Values.Any(v => v.SerializableObject != null))
                    .ToDictionary(a => a.Name, a => new Dictionary<string, object>
                    {
                        {
                            a.Type, a.Values.ToDictionary(v => v.SerializableObject,
                                v => v.Languages.ToDictionary(l => l.Key, l => l.ReadOnly))
                        }
                    }));
            if(entity.Metadata.IsMetadata)
                serDic.Add("for", metadata);

            var simple = JsonConvert.SerializeObject(serDic, JsonSerializerSettings());
            return simple;
            
        }

        private static JsonSerializerSettings JsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            return settings;
        }


        public override string Serialize(int entityId) => JsonEntity(entityId);
        public override Dictionary<int, string> Serialize(List<int> entities) => null;


        public override string Serialize(IEntity entity) => JsonEntity(entity).ToString();

        public override Dictionary<int, string> Serialize(List<IEntity> entities) => null;
        
    }
}
