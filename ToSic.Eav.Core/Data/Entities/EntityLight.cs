using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data
{
    /// <inheritdoc />
    /// <remarks>
    /// Not 100% #immutable, because the EntityId is still manipulated once in case it's a draft-entity of another entity.
    /// Not sure when/how to fix.
    /// </remarks>
    [PrivateApi("2021-09-30 hidden now, previously InternalApi_DoNotUse_MayChangeWithoutNotice this is just fyi, always use IEntity")]
	public partial class EntityLight : IEntityLight
    {
        #region Basic properties EntityId, EntityGuid, Title, Attributes, Type, Modified, etc.
        /// <inheritdoc />
        public int AppId { get; }

        /// <inheritdoc />
		public int EntityId { get; internal set; } 

        /// <inheritdoc />
		public Guid EntityGuid { get; }

        /// <inheritdoc />
        public object Title => TitleFieldName.HasValue() ? this[TitleFieldName] : null;

        [JsonIgnore]
        [PrivateApi]
        internal string TitleFieldName => _titleFieldName ?? Type.TitleFieldName;

        private readonly string _titleFieldName;

        /// <summary>
        /// List of all attributes in light-mode - single language, simple.
        /// Internal use only!
        /// </summary>
        [PrivateApi("Internal use only!")]
		public Dictionary<string, object> AttributesLight { get; }

        /// <inheritdoc />
		public IContentType Type { get; }

        /// <inheritdoc />
		public DateTime Created { get; }
        
        /// <inheritdoc />
		public DateTime Modified { get; }

        /// <inheritdoc />
        [JsonIgnore]
        public IRelationshipManager Relationships { get; }

        /// <inheritdoc />
        public ITarget MetadataFor { get; }

        /// <inheritdoc />
        public string Owner { get; }

        /// <inheritdoc />
        public int OwnerId => _ownerId.Get(() => int.TryParse(Owner.After("="), out var o) ? o : -1);
        private readonly GetOnce<int> _ownerId = new GetOnce<int>();
        #endregion

        #region direct attribute accessor using this[...]

        /// <inheritdoc />
        public object this[string attributeName]
            => AttributesLight.TryGetValue(attributeName, out var result)
                ? result
                : null;
        #endregion

        #region various constructors to create entities

        /// <remarks>
        /// Empty constructor for inheriting objects who need to build an Entity-Object
        /// </remarks>
        [PrivateApi]
        protected EntityLight() { }

        /// <summary>
        /// Create a new Entity. Used to create InMemory Entities that are not persisted to the EAV SqlStore.
        /// </summary>
        [PrivateApi]
        internal EntityLight(
            int appId, int entityId, Guid? guid, IContentType contentType, EntityPartsBuilder partsBuilder, Dictionary<string, object> rawValues, string titleFieldName = null, 
            DateTime? created = null, DateTime? modified = null, string owner = null,
            ITarget metadataFor = default)
        {
            AppId = appId;
            EntityId = entityId;
            EntityGuid = guid ?? Guid.Empty;
            Type = contentType;
            AttributesLight = rawValues;
            _titleFieldName = titleFieldName;
            MetadataFor = metadataFor ?? new Target();
            if (created.HasValue) Created = created.Value;
            if (modified.HasValue) Modified = modified.Value;
            if (!string.IsNullOrEmpty(owner)) Owner = owner;
            Relationships = partsBuilder.RelationshipManager(this);
        }


        #endregion


        #region GetBestValue and GetTitle

        /// <inheritdoc />
        public object GetBestValue(string attributeName) 
        {
            if (!AttributesLight.TryGetValue(attributeName, out var result))
            {
                var attributeNameLower = attributeName.ToLowerInvariant();
                if (attributeNameLower == Attributes.EntityFieldTitle)
                    result = Title;
                else
                    return GetInternalPropertyByName(attributeNameLower);
            }

            // map any kind of number to the one format used in other code-checks: decimal
            if (result is short
                || result is ushort
                || result is int
                || result is uint
                || result is long
                || result is ulong
                || result is float
                || result is double
                || result is decimal)
                return Convert.ToDecimal(result);

            return result;
        }



        [PrivateApi("Testing / wip #IValueConverter")]
        public TVal GetBestValue<TVal>(string name) => GetBestValue(name).ConvertOrDefault<TVal>();



        /// <summary>
        /// Get internal properties by string-name like "EntityTitle", etc.
        /// Resolves: EntityId, EntityGuid, EntityType, EntityModified
        /// Also ensure that it works in any upper/lower case
        /// </summary>
        /// <param name="attributeNameLowerInvariant"></param>
        /// <returns></returns>
        [PrivateApi]
        protected virtual object GetInternalPropertyByName(string attributeNameLowerInvariant)
        {
            switch (attributeNameLowerInvariant.ToLowerInvariant())
            {
                case Attributes.EntityFieldId:
                    return EntityId;
                case Attributes.EntityFieldGuid:
                    return EntityGuid;
                case Attributes.EntityFieldType:
                    return Type.Name;
                case Attributes.EntityFieldCreated:
                    return Created;
                case Attributes.EntityFieldOwner:   // added in v15, was missing before
                    return Owner;
                case Attributes.EntityFieldOwnerId: // new v15.03
                    return OwnerId;
                case Attributes.EntityFieldModified:
                    return Modified;
                default:
                    return null;
            }
        }


        /// <inheritdoc />
	    public string GetBestTitle() => GetBestTitle(0);

        /// <inheritdoc />
        private string GetBestTitle(int recursionCount)
        {
            var bestTitle = GetBestValue(Attributes.EntityFieldTitle);

            // in case the title is an entity-picker and has items, try to ask it for the title
            // note that we're counting recursions, just to be sure it won't loop forever
            var maybeRelationship = bestTitle as IEnumerable<IEntity>;
            if (recursionCount < 3 && (maybeRelationship?.Any() ?? false))
                bestTitle = (maybeRelationship.FirstOrDefault() as Entity)?
                    .GetBestTitle(recursionCount + 1)
                    ?? bestTitle;

            return bestTitle?.ToString();

        }



        #endregion

        #region Save/Update settings - needed when passing this object to the save-layer


        // todo: move to save options
        [PrivateApi]
        public bool PlaceDraftInBranch { get; set; }

        #endregion
    }
}
