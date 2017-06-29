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

        protected override string SerializeOne(IEntity entity) => JsonEntity(entity);

        private string JsonEntity(IEntity entity)
        {
            var header = new {V = "1.0", TS = DateTime.Now, Content = "entity"};

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

            var attributesInUse = entity.Attributes.Values
                .OrderBy(a => a.Name)
                .Where(a => a.Values.Any(v => v.SerializableObject != null))
                .ToList();

            var serDic = new Dictionary<string, object>
            {
                {"_", header},
                {"Id", entity.EntityId},
                {"Guid", entity.EntityGuid},
                {"Type", new {entity.Type.Name, Id = entity.Type.StaticName, Attributes = attributesInUse.ToDictionary(a => a.Name, a => a.Type)}}
            };
            if (entity.Attributes.Any())
                serDic.Add("Attributes", attributesInUse
                    .ToDictionary(a => a.Name, a =>  a.Values.ToDictionary(v => v.SerializableObject,
                                v => v.Languages.ToDictionary(l => l.Key, l => l.ReadOnly))));
            if(entity.Metadata.IsMetadata)
                serDic.Add("For", metadata);

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
        
    }
}
