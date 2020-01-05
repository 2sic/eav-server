using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Serialization
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    public class EntityToDictionary : IEntityTo<Dictionary<string, object>>
    {
        #region Configuration
        public bool IncludeGuid { get; set; }
        public bool IncludePublishingInfo { get; private set; }

        public bool IncludeMetadataFor { get; private set; }

        public bool ProvideIdentityTitle { get; private set; }

        /// <summary>
        /// ensure all settings are so it includes guids etc.
        /// </summary>
        public void ConfigureForAdminUse()
        {
            IncludeGuid = true;
            IncludePublishingInfo = true;
            IncludeMetadataFor = true;
            ProvideIdentityTitle = true;
        }

        #endregion

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
        public IEnumerable<Dictionary<string, object>> Convert(IEnumerable<IEntity> entities) 
            => entities.Select(GetDictionaryFromEntity);

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        public Dictionary<string, object> Convert(IEntity entity) 
            => entity == null ? null : GetDictionaryFromEntity(entity);
        

        #endregion


        /// <summary>
        /// Convert an entity into a lightweight dictionary, ready to serialize
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object> GetDictionaryFromEntity(IEntity entity)
        {
            // Convert Entity to dictionary
            // If the value is a relationship, then give those too, but only Title and Id
            var entityValues = (from d in entity.Attributes select d.Value).ToDictionary(k => k.Name, v =>
            {
				var value = entity.GetBestValue(v.Name, Languages, true);
                if (v.Type == "Entity" && value is IEnumerable<IEntity> entities)
                    return entities.Select(p => new SerializableRelationship
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

            if (IncludeMetadataFor && entity.MetadataFor.IsMetadata)
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