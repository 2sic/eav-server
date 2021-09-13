using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.ImportExport.JsonLight;
using ToSic.Eav.Logging;
using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Convert
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    [PrivateApi("Hide Implementation")]
    public partial class ConvertToJsonLight : HasLog<ConvertToJsonLight>, IConvertToJsonLight
    {
        public static string JsonKeyMetadataFor = "For"; // temp, don't know where to put this ATM
        public static string JsonKeyMetadata = "Metadata";
        public static string IdField = "Id";

        #region Constructor / DI

        public class Dependencies
        {
            public IValueConverter ValueConverter { get; }
            public IZoneCultureResolver ZoneCultureResolver { get; }

            public Dependencies(IValueConverter valueConverter, IZoneCultureResolver zoneCultureResolver)
            {
                ValueConverter = valueConverter;
                ZoneCultureResolver = zoneCultureResolver;
            }
        }

        /// <summary>
        /// Important: this constructor is used both in inherited,
        /// but also in EAV-code which uses only this object (so no inherited)
        /// This is why it must be public, because otherwise it can't be constructed from eav?
        /// </summary>
        /// <param name="dependencies"></param>
        public ConvertToJsonLight(Dependencies dependencies) : this(dependencies, "Eav.CnvE2D") { }

        protected ConvertToJsonLight(Dependencies dependencies, string logName) : base(logName)
        {
            Deps = dependencies;
        }

        private Dependencies Deps { get; }

        #endregion

        #region Configuration

        /// <inheritdoc />
        [WorkInProgressApi("Exact name not final yet")]
        public int MaxItems { get; set;  } = 0;

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
            get => _languages ?? (_languages = Deps.ZoneCultureResolver.SafeLanguagePriorityCodes());
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
        protected virtual JsonEntity GetDictionaryFromEntity(IEntity entity)
        {
            // Get serialization rules if some exist - new in 11.13
            var rules = entity as IEntitySerialization;
            var serRels = SubEntitySerialization.Stabilize(rules?.SerializeRelationships, true, true, false, true);

            // Convert Entity to dictionary
            // If the value is a relationship, then give those too, but only Title and Id
            //var entityValues = new JsonV0();

            var entityValues = entity.Attributes
                .Select(d => d.Value)
                .ToJsonV0(attribute => attribute.Name, attribute =>
                {
                    var value = entity.GetBestValue(attribute.Name, Languages);

                    // Special Case 1: Hyperlink Field which must be resolved
                    if (attribute.Type == DataTypes.Hyperlink && value is string stringValue &&
                        ValueConverterBase.CouldBeReference(stringValue))
                        return Deps.ValueConverter.ToValue(stringValue, entity.EntityGuid);

                    // Special Case 2: Entity-List
                    if (attribute.Type == DataTypes.Entity && value is IEnumerable<IEntity> entities)
                        return serRels.Serialize == true ? CreateListOfSubEntities(entities, serRels) : null;

                    // Default: Normal Value
                    return value;
                });
                //.ToList()
                //.ForEach(action: attribute => entityValues[attribute.Name] = GetJsonV0Value(entity, attribute, serRels));
            
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
                if (!entityValues.ContainsKey(Attributes.TitleNiceName))
                    entityValues.Add(Attributes.TitleNiceName, entity.GetBestTitle(Languages));
                
            AddDateInformation(entity, entityValues, rules);

            return entityValues;
        }

    }
}