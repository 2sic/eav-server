using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Serialization;
using ToSic.Lib.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    [PrivateApi("Hide Implementation")]
    public partial class ConvertToEavLight : HasLog, IConvertToEavLight
    {
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
        public ConvertToEavLight(Dependencies dependencies) : this(dependencies, "Eav.CnvE2D") { }

        private ConvertToEavLight(Dependencies dependencies, string logName) : base(logName)
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
        
        [PrivateApi] public MetadataForSerialization MetadataFor { get; private set; } = new MetadataForSerialization();
        [PrivateApi] public ISubEntitySerialization Metadata { get; private set; } = new SubEntitySerialization();

        private bool WithEditInfos { get; set; }

        /// <inheritdoc/>
        public void ConfigureForAdminUse()
        {
            WithGuid = true;
            WithPublishing = true;
            MetadataFor = new MetadataForSerialization { Serialize = true };
            Metadata = new SubEntitySerialization { Serialize = true, SerializeId = true, SerializeTitle = true, SerializeGuid = true };
            WithEditInfos = true;
        }

        #endregion

        #region Language

        public string[] Languages
        {
            get => _languages ?? (_languages = Deps.ZoneCultureResolver.SafeLanguagePriorityCodes());
            set => _languages = value;
        }

        [PrivateApi("not public ATM")]
        public TypeSerialization Type { get; set; } = new TypeSerialization();

        private string[] _languages;

        #endregion

        #region Constants

        // TODO: the _Title is probably never used in JS but we must verify
        public const string InternalTitleField = "_Title";
        public const string InternalTypeField = "_Type";

        #endregion


        /// <summary>
        /// Convert an entity into a lightweight dictionary, ready to serialize
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [PrivateApi]
        protected virtual EavLightEntity GetDictionaryFromEntity(IEntity entity)
        {
            // Get serialization rules if some exist - new in 11.13
            // var rules = entity as IEntitySerialization;
            var rules = entity.GetDecorator<EntitySerializationDecorator>();
            var serRels = SubEntitySerialization.Stabilize(rules?.SerializeRelationships, true, true, false, true);

            // Convert Entity to dictionary
            // If the value is a relationship, then give those too, but only Title and Id
            var entityValues = entity.Attributes
                .Select(d => d.Value)
                .ToEavLight(attribute => attribute.Name, attribute =>
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

            // todo: verify what happens with null-values on the relationships, maybe we should filter them out again?

            // New 12.05 - drop null values
            if(rules != null) OptimizeRemoveEmptyValues(rules, entityValues);

            AddIdAndGuid(entity, entityValues, rules);

            if (WithPublishing) AddPublishingInformation(entity, entityValues);

            AddMetadataAndFor(entity, entityValues, rules);

            // Special edit infos - _Title (old, maybe not needed), Stats, EditInfo for read-only etc.
            if (WithEditInfos)
            {
                // this internal _Title field is probably not used much any more, so there is no rule for it
                // Probably remove at some time in near future, once verified it's not used in the admin-front-end
                try { entityValues.Add(InternalTitleField, entity.GetBestTitle(Languages)); }
                catch { /* ignore */ }

                AddStatistics(entity, entityValues);
                AddEditInfo(entity, entityValues);
            }


            // Include title field, if there is not already one in the dictionary
            if(rules?.SerializeTitle ?? true)
                if (!entityValues.ContainsKey(Attributes.TitleNiceName))
                    entityValues.Add(Attributes.TitleNiceName, entity.GetBestTitle(Languages));
                
            AddDateInformation(entity, entityValues, rules);

            if (Type.Serialize) entityValues.Add(InternalTypeField, new JsonType(entity, Type.WithDescription));

            return entityValues;
        }

        private void OptimizeRemoveEmptyValues(EntitySerializationDecorator rules, EavLightEntity entityValues)
        {
            if (rules == null) return;
            var dropNulls = rules.RemoveNullValues;
            var dropZeros = rules.RemoveZeroValues;
            var dropEmptyStrings = rules.RemoveEmptyStringValues;
            var dropBoolFalse = rules.RemoveBoolFalseValues;

            try
            {
                if (dropNulls)
                    entityValues
                        .Where(vp => vp.Value == null)
                        .ToList()
                        .ForEach(vp => entityValues.Remove(vp.Key));
            }
            catch (Exception e)
            {
                Log.A("Couldn't drop NULL values, will ignore and continue");
                Log.Ex(e);
            }

            try
            {
                if (dropZeros)
                    entityValues
                        .Where(vp =>
                            vp.Value is IConvertible convertible && convertible.IsNumeric() &&
                            System.Convert.ToDouble(convertible) == 0)
                        .ToList()
                        .ForEach(vp => entityValues.Remove(vp.Key));
            }
            catch (Exception e)
            {
                Log.A("Couldn't drop ZERO values, will ignore and continue");
                Log.Ex(e);
            }

            try
            {
                if (dropEmptyStrings)
                    entityValues
                        .Where(vp => vp.Value as string == string.Empty)
                        .ToList()
                        .ForEach(vp => entityValues.Remove(vp.Key));
            }
            catch (Exception e)
            {
                Log.A("Couldn't drop EMPTY string values, will ignore and continue");
                Log.Ex(e);
            }

            try
            {
                if (dropBoolFalse)
                    entityValues
                        .Where(vp => vp.Value is bool boolVal && boolVal == false)
                        .ToList()
                        .ForEach(vp => entityValues.Remove(vp.Key));
            }
            catch (Exception e)
            {
                Log.A("Couldn't drop FALSE boolean values, will ignore and continue");
                Log.Ex(e);
            }
        }
    }
}