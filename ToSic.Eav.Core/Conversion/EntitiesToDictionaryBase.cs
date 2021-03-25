using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.Conversion
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public abstract partial class EntitiesToDictionaryBase : HasLog<EntitiesToDictionaryBase>, IEntitiesTo<Dictionary<string, object>>
    {
        public static string JsonKeyMetadataFor = "For"; // temp, don't know where to put this ATM
        public static string JsonKeyMetadata = "Metadata";
        public static string IdField = "Id";

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



        /// <summary>
        /// Convert an entity into a lightweight dictionary, ready to serialize
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [PrivateApi]
        protected virtual Dictionary<string, object> GetDictionaryFromEntity(IEntity entity)
        {
            // Get serialization rules if some exist - new in 11.13
            var rules = entity as IEntitySerialization;
            var serRels = SubEntitySerialization.Stabilize(rules?.SerializeRelationships, true, true, false, true);

            // Convert Entity to dictionary
            // If the value is a relationship, then give those too, but only Title and Id
            var entityValues = entity.Attributes
                .Select(d => d.Value)
                .ToDictionary(k => k.Name, v =>
                {
                    var value = entity.GetBestValue(v.Name, Languages);

                    // Special Case 1: Hyperlink Field which must be resolved
                    if (v.Type == Constants.DataTypeHyperlink && value is string stringValue &&
                        ValueConverterBase.CouldBeReference(stringValue))
                        return ValueConverter.ToValue(stringValue, entity.EntityGuid);

                    // Special Case 2: Entity-List
                    if (v.Type == Constants.DataTypeEntity && value is IEnumerable<IEntity> entities)
                        return serRels.Serialize == true ? CreateListOfSubEntities(entities, serRels) : null;

                    // Default: Normal Value
                    return value;

                }, StringComparer.OrdinalIgnoreCase);
            
            // todo: verify what happens with null-values on the relationships, maybe we should filter them out again?


            AddIdAndGuid(entity, entityValues, rules);

            if (WithPublishing) AddPublishingInformation(entity, entityValues);

            AddMetadataAndFor(entity, entityValues, rules);

            // this internal _Title field is probably not used much any more, so there is no rule for it
            if(WithTitle)
                try { entityValues.Add("_Title", entity.GetBestTitle(Languages)); }
                catch { /* ignore */ }

            // Stats are only used in special cases, so there is no rule for it
            if(WithStats) AddStatistics(entity, entityValues);


            // Include title field, if there is not already one in the dictionary
            if(rules?.SerializeTitle ?? true)
                if (!entityValues.ContainsKey(Constants.SysFieldTitle))
                    entityValues.Add(Constants.SysFieldTitle, entity.GetBestTitle(Languages));
                
            AddDateInformation(entity, entityValues, rules);

            return entityValues;
        }



    }
}