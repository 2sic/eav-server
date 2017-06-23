using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Represents an Entity
	/// </summary>
	public class EntityLight : IEntityLight
    {
	    #region Basic properties EntityId, EntityGuid, Title, Attributes, Type, Modified, etc.
        /// <summary>
        /// Id as an int
        /// </summary>
		public int EntityId { get; internal set; } 

        /// <summary>
        /// Id as GUID
        /// </summary>
		public Guid EntityGuid { get; internal set; }

        /// <summary>
        /// Offical title of this content-item
        /// </summary>
        public object Title => TitleFieldName == null ? null : this[TitleFieldName];

        [ScriptIgnore]
        internal string TitleFieldName;

        /// <summary>
        /// List of all attributes
        /// </summary>
		public Dictionary<string, object> Attributes { get; set; }

        /// <summary>
        /// Type-definition of this content-item
        /// </summary>
		public IContentType Type { get; internal set; }

        /// <summary>
        /// Modified date/time
        /// </summary>
		public DateTime Modified { get; internal set; }

        /// <summary>
        /// Relationship-helper object, important to navigate to children and parents
        /// </summary>
		[ScriptIgnore]
		public IRelationshipManager Relationships { get; internal set; }

        public IMetadata Metadata { get; internal set; }

        /// <summary>
        /// Owner of this entity
        /// </summary>
        public string Owner { get; internal set; }
        #endregion

        #region direct attribute accessor using this[...]
        /// <summary>
        /// Shorhand accessor to retrieve an attribute
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public object this[string attributeName] => Attributes.ContainsKey(attributeName) ? Attributes[attributeName] : null;
        #endregion

        #region various constructors to create entities

        /// <summary>
        /// Empty constructor for inheriting objects who need to build an Entity-Object
        /// </summary>
        protected EntityLight() { }

        /// <summary>
        /// Create a new Entity. Used to create InMemory Entities that are not persisted to the EAV SqlStore.
        /// </summary>
        public EntityLight(int entityId, string contentTypeName, Dictionary<string, object> values, string titleAttribute = null, DateTime? modified = null)
        {
            EntityId = entityId;
            Type = new ContentType(contentTypeName);
            Attributes = values;//.ConvertToAttributes();
            try
            {
                if (titleAttribute != null)
                    TitleFieldName = titleAttribute;
                    //Title = Attributes[titleAttribute];
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"The Title Attribute with Name \"{titleAttribute}\" doesn't exist in the Entity-Attributes.");
            }
            Metadata = new Metadata();
            //IsPublished = true;
            if (modified.HasValue)
                Modified = modified.Value;
            Relationships = new RelationshipManager(this, new EntityRelationshipItem[0]);
        }

        /// <summary>
        /// Create a brand new Entity. 
        /// Mainly used for entities which are created for later saving
        /// </summary>
        public EntityLight(Guid entityGuid, string contentTypeName, Dictionary<string, object> values) : this(0, contentTypeName, values)
        {
            EntityGuid = entityGuid;
        }

        #endregion


        #region GetBestValue and GetTitle


        /// <summary>
        /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
        /// Automatically resolves the language-variations as well based on the list of preferred languages
        /// </summary>
        /// <param name="attributeName">Name of the attribute or virtual attribute</param>
        /// <param name="resolveHyperlinks"></param>
        /// <returns>An object OR a null - for example when retrieving the title and no title exists</returns>
        public object GetBestValue(string attributeName, bool resolveHyperlinks = false)
        {
            object result;

            if (Attributes.ContainsKey(attributeName))
                result = Attributes[attributeName];
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


        /// <summary>
        /// Best way to get the current entities title
        /// </summary>
        /// <returns>The entity title as a string</returns>
	    public string GetBestTitle() => GetBestTitle(0);

        public string GetBestTitle(int recursionCount)
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
