using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Conversion
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public abstract class EntitiesToDictionaryBase : HasLog<EntitiesToDictionaryBase>, IEntitiesTo<Dictionary<string, object>>
    {
        #region Constructor / DI

        protected EntitiesToDictionaryBase(IValueConverter valueConverter, IZoneCultureResolver cultureResolver, string logName) : base(logName)
        {
            _cultureResolver = cultureResolver;
            ValueConverter = valueConverter;
        }
        private readonly IZoneCultureResolver _cultureResolver;
        protected IValueConverter ValueConverter { get; }

        #endregion

        #region Configuration
        /// <inheritdoc/>
        public bool WithGuid { get; set; }
        /// <inheritdoc/>
        public bool WithPublishing { get; private set; }
        /// <inheritdoc/>
        public bool WithMetadataFor { get; private set; }
        /// <inheritdoc/>
        public bool WithTitle { get; private set; }

        private bool WithStats { get; set; }

        /// <inheritdoc/>
        public void ConfigureForAdminUse()
        {
            WithGuid = true;
            WithPublishing = true;
            WithMetadataFor = true;
            WithTitle = true;
            WithStats = true;
        }

        #endregion

        #region Language

        public string[] Languages
        {
            get => _languages ?? (_languages = _cultureResolver.SafeLanguagePriorityCodes());
            set => _languages = value;
        }
        private string[] _languages;

        #endregion

        #region Many variations of the Prepare-Statement expecting various kinds of input

        /// <inheritdoc/>
        public IEnumerable<Dictionary<string, object>> Convert(IEnumerable<IEntity> entities)
        {
            var wrapLog = Log.Call(useTimer: true);
            var result = entities.Select(GetDictionaryFromEntity).ToList();
            wrapLog("ok");
            return result;
        }

        /// <inheritdoc/>
        public Dictionary<string, object> Convert(IEntity entity)
        {
            var wrapLog = Log.Call(useTimer: true);
            var result = entity == null ? null : GetDictionaryFromEntity(entity);
            wrapLog("ok");
            return result;
        }

        #endregion


        /// <summary>
        /// Convert an entity into a lightweight dictionary, ready to serialize
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [PrivateApi]
        protected virtual Dictionary<string, object> GetDictionaryFromEntity(IEntity entity)
        {
            // Convert Entity to dictionary
            // If the value is a relationship, then give those too, but only Title and Id
            var entityValues = (from d in entity.Attributes select d.Value).ToDictionary(k => k.Name, v =>
            {
				var value = entity.GetBestValue(v.Name, Languages);
                if (v.Type == Constants.DataTypeHyperlink && value is string stringValue && ValueConverterBase.CouldBeReference(stringValue))
                    return ValueConverter.ToValue(stringValue, entity.EntityGuid);

                if (v.Type == Constants.DataTypeEntity && value is IEnumerable<IEntity> entities)
                    return entities.Select(p => new RelationshipReference
                    {
                        Id = p?.EntityId,
                        Title = p?.GetBestTitle(Languages)
                    }).ToList();
                return value;
				
            }, StringComparer.OrdinalIgnoreCase);

            // Add Id and Title
            // ...only if these are not already existing with this name in the entity itself as an internal value
            const string IdKey = "Id";
            const string GuidKey = "Guid";
            if (entityValues.ContainsKey(IdKey)) entityValues.Remove(IdKey);
            entityValues.Add(IdKey, entity.EntityId);

            if (WithGuid)
            {
                if (entityValues.ContainsKey(GuidKey)) entityValues.Remove(GuidKey);
                entityValues.Add(GuidKey, entity.EntityGuid);
            }

            if (WithPublishing)
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

            if (WithMetadataFor && entity.MetadataFor.IsMetadata)
                entityValues.Add("Metadata", entity.MetadataFor);

            if(WithTitle)
                try { entityValues.Add("_Title", entity.GetBestTitle(Languages)); }
                catch { /* ignore */ }

            if(WithStats)
                try
                {
                    entityValues.Add("_Used", entity.Parents().Count);
                    entityValues.Add("_Uses", entity.Children().Count);
                    entityValues.Add("_Permissions", new {Count = entity.Metadata.Permissions.Count()});
                }
                catch { /* ignore */ }


            // Include title field, if there is not already one in the dictionary
            if (!entityValues.ContainsKey(Constants.SysFieldTitle))
                entityValues.Add(Constants.SysFieldTitle, entity.GetBestTitle(Languages));
                
            // Include modified field, if there is not already one in the dictionary
            if(!entityValues.ContainsKey(Constants.SysFieldModified))
                entityValues.Add(Constants.SysFieldModified, entity.Modified);
            
            // Include created field, if there is not already one in the dictionary
            if(!entityValues.ContainsKey(Constants.SysFieldCreated))
                entityValues.Add(Constants.SysFieldCreated, entity.Created);

            return entityValues;
        }


    }
}