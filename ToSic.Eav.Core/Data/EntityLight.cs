using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
	/// <inheritdoc />
	public class EntityLight : IEntityLight
    {
	    #region Basic properties EntityId, EntityGuid, Title, Attributes, Type, Modified, etc.
        public int AppId { get; internal set; }
        /// <inheritdoc />
		public int EntityId { get; internal set; } 

        /// <inheritdoc />
		public Guid EntityGuid { get; internal set; }

        /// <inheritdoc />
        public object Title => TitleFieldName == null ? null : this[TitleFieldName];

        [ScriptIgnore]
        internal string TitleFieldName;

        /// <summary>
        /// List of all attributes
        /// </summary>
		protected Dictionary<string, object> LightAttributesForInternalUseOnlyForNow { get; set; }

        /// <inheritdoc />
		public IContentType Type { get; internal set; }

        /// <inheritdoc />
		public DateTime Modified { get; internal set; }

        /// <inheritdoc />
		[ScriptIgnore]
		public IRelationshipManager Relationships { get; internal set; }

        public IMetadata Metadata { get; internal set; }

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
        protected EntityLight() { }

        /// <summary>
        /// Create a new Entity. Used to create InMemory Entities that are not persisted to the EAV SqlStore.
        /// </summary>
        public EntityLight(int appId, int entityId, object contentType, Dictionary<string, object> values, string titleAttribute = null, DateTime? modified = null)
        {
            AppId = appId;
            EntityId = entityId;
            SetContentTypeFromNameOrObject(appId, contentType);
            LightAttributesForInternalUseOnlyForNow = values;//.ConvertToAttributes();
            try
            {
                if (titleAttribute != null)
                    TitleFieldName = titleAttribute;
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"The Title Attribute with Name \"{titleAttribute}\" doesn't exist in the Entity-Attributes.");
            }
            Metadata = new Metadata();
            if (modified.HasValue)
                Modified = modified.Value;
            Relationships = new RelationshipManager(this, new EntityRelationshipItem[0]);
        }

        private void SetContentTypeFromNameOrObject(int appId, object contentType)
        {
            switch (contentType)
            {
                case IContentType _:
                    Type = contentType as IContentType;
                    break;
                case string _:
                    Type = new ContentType(appId, contentType as string);
                    break;
                default:
                    throw new Exception(
                        $"content type should be string or of type IContentType - it's {contentType.GetType().FullName}");
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a brand new Entity. 
        /// Mainly used for entities which are created for later saving
        /// </summary>
        public EntityLight(int appId, Guid entityGuid, object contentType, Dictionary<string, object> values) : this(appId, 0, contentType, values)
        {
            EntityGuid = entityGuid;
        }

        #endregion


        #region GetBestValue and GetTitle


        /// <inheritdoc />
        public object GetBestValue(string attributeName, bool resolveHyperlinks = false)
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

            if (resolveHyperlinks)
                result = TryToResolveLink(result);

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

        /// <summary>
        /// Get internal properties by string-name like "EntityTitle", etc.
        /// Resolves: EntityId, EntityGuid, EntityType, EntityModified
        /// Also ensure that it works in any upper/lower case
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
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

        protected static object TryToResolveLink(object result)
        {
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            return result is string
                ? Factory.Resolve<IEavValueConverter>().Convert(ConversionScenario.GetFriendlyValue, Constants.Hyperlink, (string)result)
                : result;
        }


        /// <inheritdoc />
	    public string GetBestTitle() => GetBestTitle(0);

        private string GetBestTitle(int recursionCount)
        {
            var bestTitle = GetBestValue(Constants.EntityFieldTitle);

            // in case the title is an entity-picker and has items, try to ask it for the title
            // note that we're counting recursions, just to be sure it won't loop forever
            var maybeRelationship = bestTitle as EntityRelationship;
            if (recursionCount < 3 && (maybeRelationship?.Any() ?? false))
                bestTitle = (maybeRelationship.FirstOrDefault() as Entity)?
                    .GetBestTitle(recursionCount + 1)
                    ?? bestTitle;

            return bestTitle?.ToString();

        }



        #endregion

        #region Save/Update settings - needed when passing this object to the save-layer


        // todo: move to save options
        public bool PlaceDraftInBranch { get; set; }

        #endregion
    }
}
