using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Serializers
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    public class SerializerHelper
    {
        #region Configuration
        public bool IncludeGuid { get; set; }
        protected bool IncludePublishingInfo { get; private set; }

        protected bool IncludeMetadata { get; private set; }

        public bool IncludeAllEditingInfos { get; set; }

        protected bool ProvideIdentityTitle { get; private set; }

        /// <summary>
        /// ensure all settings are so it includes guids etc.
        /// </summary>
        public void ConfigureForAdminUse()
        {
            IncludeGuid = true;
            IncludePublishingInfo = true;
            IncludeMetadata = true;
            ProvideIdentityTitle = true;
        }

        #endregion

        //public SerializerHelper()
        //{
            // Ensure that date-times are sent in the Zulu-time format (UTC) and not with offsets which causes many problems during round-trips
            // 2017-06-07 2dm: can't use this setting outside of web...
            // must find a solution... GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            //GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        //}

        #region Language
        private string[] _langs;

        public string[] Languages
        {
            get => _langs ?? (_langs = new[] { Thread.CurrentThread.CurrentCulture.Name });
            set => _langs = value;
        }
        #endregion

        #region Many variations of the Prepare-Statement expecting various kinds of input
 
        

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        /// <remarks>
        ///     note that this could be in use on webAPIs and scripts
        ///     so even if it looks un-used, it must stay available
        /// </remarks>
        [Obsolete("Try to use the List-overload instead of the dictionary overload")]
        public IEnumerable<Dictionary<string, object>> Prepare(IDictionary<int, IEntity> list) 
            => list.Select(c => GetDictionaryFromEntity(c.Value));

        public IEnumerable<Dictionary<string, object>> Prepare(IEnumerable<IEntity> entities) 
            => entities.Select(GetDictionaryFromEntity);

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        public Dictionary<string, object> Prepare(IEntity entity) 
            => entity == null ? null : GetDictionaryFromEntity(entity);
        

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
                entityValues.Add(Constants.RepoIdInternalField, entity.RepositoryId);
                entityValues.Add(Constants.IsPublishedField, entity.IsPublished);
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

            if (IncludeMetadata && entity.MetadataFor.IsMetadata)
                entityValues.Add("Metadata", entity.MetadataFor);

            if(ProvideIdentityTitle)
                try { entityValues.Add("_Title", entity.GetBestTitle(Languages)); }
                catch { /* ignore */ }

            // todo: unclear if this is still needed, but it would be risky to remove, without analyzing all scripts
            if (!entityValues.ContainsKey("Title"))
                try // there are strange cases where the title is missing, then just ignore this
                {
                    entityValues.Add("Title", entity.GetBestTitle(Languages));
                }
                catch { /* ignore */ }

            return entityValues;
        }
    }
}