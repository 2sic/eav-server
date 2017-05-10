using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http;
using Newtonsoft.Json;
using ToSic.Eav.DataSources;

// ReSharper disable once CheckNamespace
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
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;//.Unspecified;
            //GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

        #region Language
        private string[] _langs;

        public string[] Languages
        {
            get { return _langs ?? (_langs = new[] { Thread.CurrentThread.CurrentCulture.Name }); }
            set { _langs = value; }
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
                .ToDictionary(k => k, s => source.Out[s].LightList.Select(GetDictionaryFromEntity)
            );

            return y;
        }

        /// <summary>
        /// Returns an object that represents an IDataSource, but is serializable. If streamsToPublish is null, it will return all streams.
        /// </summary>
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Prepare(IDataSource source, string streamsToPublish)
            => Prepare(source, streamsToPublish.Split(','));

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        public IEnumerable<Dictionary<string, object>> Prepare(IDataStream stream)
            => Prepare(stream.LightList);
        

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        [Obsolete("Try to use the List-overload instead of the dictionary overload")]
        public IEnumerable<Dictionary<string, object>> Prepare(IDictionary<int, IEntity> list)
            => list.Select(c => GetDictionaryFromEntity(c.Value));

        public IEnumerable<Dictionary<string, object>> Prepare(IEnumerable<IEntity> entities)
            => entities.Select(GetDictionaryFromEntity);


        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        public Dictionary<string, object> Prepare(IEntity entity)
            => (entity == null) ? null : GetDictionaryFromEntity(entity);
        

        #endregion


        /// <summary>
        /// Convert an entity into a lightweight dictionary, ready to serialize
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object> GetDictionaryFromEntity(IEntity entity)
        {
            // var lngs = Languages;// new[] {Languages};
            // Convert Entity to dictionary
            // If the value is a relationship, then give those too, but only Title and Id
            var entityValues = (from d in entity.Attributes select d.Value).ToDictionary(k => k.Name, v =>
            {
				var value = entity.GetBestValue(v.Name, Languages, true);
                if (v.Type == "Entity" && value is Data.EntityRelationship)
                    return ((Data.EntityRelationship) value).Select(p => new SerializableRelationship
                    {
                        Id = p?.EntityId,
                        Title = p?.GetBestTitle(Languages)
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
                if (entity.IsPublished && entity.GetDraft() != null)
                {
                    // do a check if there was a field called Published, which we must remove for this to work
                    if (entityValues.ContainsKey(Constants.DraftEntityField))
                        entityValues.Remove(Constants.DraftEntityField);
                    entityValues.Add(Constants.DraftEntityField, new
                    {
                        entity.GetDraft().RepositoryId,
                    });
                }
                if (!entity.IsPublished & entity.GetPublished() != null)
                {
                    // do a check if there was a field called Published, which we must remove for this to work
                    if (entityValues.ContainsKey(Constants.PublishedEntityField))
                        entityValues.Remove(Constants.PublishedEntityField);
                    entityValues.Add(Constants.PublishedEntityField, new
                    {
                        entity.GetPublished().RepositoryId,
                    });
                }
            }

            if (IncludeMetadata)
            {
                if(entity.Metadata.HasMetadata)
                    entityValues.Add("Metadata", entity.Metadata);
            }

            if (!entityValues.ContainsKey("Title"))
                try // there are strange cases where the title is missing, then just ignore this
                {
                    entityValues.Add("Title", entity.GetBestTitle(Languages));
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }

            return entityValues;
        }
    }
}