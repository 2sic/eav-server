using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Basic;

namespace ToSic.Eav.Data
{
    /// <inheritdoc />
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi, always use IEntity")]
	public class EntityLight : IEntityLight
    {
        #region Basic properties EntityId, EntityGuid, Title, Attributes, Type, Modified, etc.
        /// <inheritdoc />
        public int AppId { get; internal set; }

        /// <inheritdoc />
		public int EntityId { get; internal set; } 

        /// <inheritdoc />
		public Guid EntityGuid { get; internal set; }

        /// <inheritdoc />
        public object Title => TitleFieldName == null ? null : this[TitleFieldName];

        [Newtonsoft.Json.JsonIgnore]
        [PrivateApi]
        internal string TitleFieldName;

        /// <summary>
        /// List of all attributes
        /// </summary>
        [PrivateApi]
		protected Dictionary<string, object> LightAttributesForInternalUseOnlyForNow { get; set; }

        /// <inheritdoc />
		public IContentType Type { get; internal set; }

        /// <inheritdoc />
		public DateTime Modified { get; internal set; }

        /// <inheritdoc />
        [Newtonsoft.Json.JsonIgnore]
        public IRelationshipManager Relationships { get; internal set; }

        /// <inheritdoc />
        public ITarget MetadataFor { get; internal set; }

        /// <inheritdoc />
        public string Owner { get; internal set; }
        #endregion

        #region direct attribute accessor using this[...]

        /// <inheritdoc />
        public object this[string attributeName]
            => LightAttributesForInternalUseOnlyForNow.ContainsKey(attributeName)
                ? LightAttributesForInternalUseOnlyForNow[attributeName]
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
        internal EntityLight(int appId, int entityId, Guid? guid, IContentType contentType, Dictionary<string, object> values, string titleAttribute = null, DateTime? modified = null)
        {
            AppId = appId;
            EntityId = entityId;
            if(guid != null) EntityGuid = guid.Value;
            Type = contentType;// CreateContentType(appId, contentType);
            LightAttributesForInternalUseOnlyForNow = values;
            try
            {
                if (titleAttribute != null)
                    TitleFieldName = titleAttribute;
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"The Title Attribute with Name \"{titleAttribute}\" doesn't exist in the Entity-Attributes.");
            }
            MetadataFor = new Metadata.Target();
            if (modified.HasValue)
                Modified = modified.Value;
            Relationships = new RelationshipManager(this, null, null);
        }


        #endregion


        #region GetBestValue and GetTitle

        [PrivateApi("Testing / wip #IValueConverter")]
        public object GetBestValue(string attributeName) => GetBestValue(attributeName, false);

        /// <inheritdoc />
        public object GetBestValue(string attributeName, bool resolveHyperlinks /*= false*/)
        {
            object result;

            if (LightAttributesForInternalUseOnlyForNow.ContainsKey(attributeName))
                result = LightAttributesForInternalUseOnlyForNow[attributeName];
            else switch (attributeName.ToLower())
            {
                case Constants.EntityFieldTitle:
                    result = Title;
                    break;
                default:
                    return GetInternalPropertyByName(attributeName);
            }

            if (resolveHyperlinks && result is string strResult)
                result = TryToResolveLink(EntityGuid, strResult);

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

        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, bool resolveHyperlinks/* = false*/) 
            => ChangeTypeOrDefault<TVal>(GetBestValue(name, resolveHyperlinks));

        [PrivateApi("Testing / wip #IValueConverter")]
        public TVal GetBestValue<TVal>(string name) => ChangeTypeOrDefault<TVal>(GetBestValue(name));

        /// <summary>
        /// Will try to convert an object to a type, or if not valid
        /// return the default (null, zero, etc.) of a type
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="found"></param>
        /// <returns></returns>
        /// <remarks>
        /// also used by Entity.cs, because that uses it's own GetBestValue(...)
        /// </remarks>
        [PrivateApi]
        protected static TVal ChangeTypeOrDefault<TVal>(object found)
        {
            if (found == null)
                return default(TVal);

            try
            {
                return (TVal) Convert.ChangeType(found, typeof(TVal));
            }
            catch
            {
                return default(TVal);
            }
        }

        /// <summary>
        /// Get internal properties by string-name like "EntityTitle", etc.
        /// Resolves: EntityId, EntityGuid, EntityType, EntityModified
        /// Also ensure that it works in any upper/lower case
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        [PrivateApi]
        protected object GetInternalPropertyByName(string attributeName)
        {
            switch (attributeName.ToLower())
            {
                case Constants.EntityFieldId:
                    return EntityId;
                case Constants.EntityFieldGuid:
                    return EntityGuid;
                case Constants.EntityFieldType:
                    return Type.Name;
                case Constants.EntityFieldModified:
                    return Modified;
                default:
                    return null;
            }
        }

        [PrivateApi]
        protected static string TryToResolveLink(Guid itemGuid, string result)
        {
            if (!BasicValueConverter.CouldBeReference(result)) return result;
            return Factory.Resolve<IValueConverter>().ToValue(result, itemGuid);
        }

        /// <inheritdoc />
	    public string GetBestTitle() => GetBestTitle(0);

        /// <inheritdoc />
        private string GetBestTitle(int recursionCount)
        {
            var bestTitle = GetBestValue(Constants.EntityFieldTitle);

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
