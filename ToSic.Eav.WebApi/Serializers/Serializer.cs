using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Serializers
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    public class Serializer
    {
        #region Configuration
        public bool IncludeGuid { get; set; }
        public bool IncludePublishingInfo { get; set; }

        public bool IncludeMetadata { get; set; }

        public bool IncludeAllEditingInfos { get; set; }

        /// <summary>
        /// ensure all settings are so it includes guids etc.
        /// </summary>
        public void ConfigureForAdminUse()
        {
            IncludeGuid = true;
            IncludePublishingInfo = true;
            IncludeMetadata = true;
        }

        #endregion

        public Serializer()
        {
            // Ensure that date-times are sent in the Zulu-time format (UTC) and not with offsets which causes many problems during round-trips
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;//.Unspecified;
        }

        #region Language

        private string _Language = "";

        private string Language
        {
            get
            {
                if(_Language == "")
                    _Language = Thread.CurrentThread.CurrentCulture.Name;
                return _Language;
            }
        }

        #endregion
        #region Many variations of the Prepare-Statement expecting various kinds of input
        /// <summary>
        /// Returns an object that represents an IDataSource, but is serializable. If streamsToPublish is null, it will return all streams.
        /// </summary>
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Prepare(IDataSource source, IEnumerable<string> streamsToPublish = null)
        {
            if (streamsToPublish == null)
                streamsToPublish = source.Out.Select(p => p.Key);

            var y = streamsToPublish.Where(k => source.Out.ContainsKey(k))
                .ToDictionary(k => k, s => source.Out[s].LightList.Select(c => GetDictionaryFromEntity(c, Language))
            );

            return y;
        }

        /// <summary>
        /// Returns an object that represents an IDataSource, but is serializable. If streamsToPublish is null, it will return all streams.
        /// </summary>
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Prepare(IDataSource source, string streamsToPublish)
        {
            return Prepare(source, streamsToPublish.Split(','));
        }

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        public IEnumerable<Dictionary<string, object>> Prepare(IDataStream stream)
        {
            return Prepare(stream.LightList);  
        }

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        [Obsolete ("Try to use the List-overload instead of the dictionary overload")]
        public IEnumerable<Dictionary<string, object>> Prepare(IDictionary<int, IEntity> list)
        {
            return list.Select(c => GetDictionaryFromEntity(c.Value, Language));
        }


        public IEnumerable<Dictionary<string, object>> Prepare(IEnumerable<IEntity> entities)
        {
            return entities.Select(c => GetDictionaryFromEntity(c, Language));
        }

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        public Dictionary<string, object> Prepare(IEntity entity)
        {
            return GetDictionaryFromEntity(entity, Language);
        }

        #endregion


        /// <summary>
        /// Convert an entity into a lightweight dictionary, ready to serialize
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object> GetDictionaryFromEntity(IEntity entity, string language)
        {
            // Convert Entity to dictionary
            // If the value is a relationship, then give those too, but only Title and Id
            var entityValues = (from d in entity.Attributes select d.Value).ToDictionary(k => k.Name, v =>
            {
				var value = entity.GetBestValue(v.Name, new[] { language }, true);
                if (v.Type == "Entity" && value is Data.EntityRelationship)
                    return ((Data.EntityRelationship) value).Select(p => new
                    {
                        Id = p?.EntityId,
                        Title = p?.GetBestValue("EntityTitle")?.ToString()
                    }).ToList();
				return value;
				
            }, StringComparer.OrdinalIgnoreCase);

            // Add Id and Title
            // ...only if these are not already existing with this name in the entity itself as an internal value
            if (entityValues.ContainsKey("Id")) entityValues.Remove("Id");
            entityValues.Add("Id", entity.EntityId);

            if (IncludeGuid)
            {
                if (entityValues.ContainsKey("Guid")) entityValues.Remove("Guid");
                entityValues.Add("Guid", entity.EntityGuid);
            }

            if (IncludePublishingInfo)
            {
                entityValues.Add("RepositoryId", entity.RepositoryId);
                entityValues.Add("IsPublished", entity.IsPublished);
                if(entity.IsPublished && entity.GetDraft() != null)
                    entityValues.Add("Draft", new
                    {
                        entity.GetDraft().RepositoryId,
                    });
                if(!entity.IsPublished & entity.GetPublished() != null)
                    entityValues.Add("Published", new
                    {
                        entity.GetPublished().RepositoryId, 
                    });
            }

            if (IncludeMetadata)
            {
                if(entity.Metadata.HasMetadata)
                    entityValues.Add("Metadata", entity.Metadata);
            }

            if (!entityValues.ContainsKey("Title"))
                try // there are strange cases where the title is missing, then just ignore this
                {
                    entityValues.Add("Title", entity.GetBestValue("EntityTitle", new []{language}, true));
                }
                catch
                {
                }

            return entityValues;
        }
    }
}