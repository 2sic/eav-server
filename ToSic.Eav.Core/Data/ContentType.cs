using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Content Type
    /// </summary>
    public class ContentType : IContentType, IContentTypeShareable
    {
        #region simple properties

        public int AppId { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// Static name - can be a GUID or a system-term for special types
        /// </summary>
        public string StaticName { get; protected set; }

        public string Description { get; protected set;  }
        /// <summary>
        /// What this content-types if for, if it's a system type or something
        /// </summary>
        public string Scope { get; protected set; }

        /// <summary>
        /// Internal Id of the attribute-set of this content-type. Don't worry about this one, you probably won't understand it and that's ok. 
        /// </summary>
        public int ContentTypeId { get; }

        #region Sharing Content Types
        public int? ParentConfigurationId { get; }
        public int ParentConfigurationAppId { get; }
        public int ParentConfigurationZoneId { get; }

        public bool AlwaysShareConfiguration { get; protected set; }
        #endregion

        /// <summary>
        /// Dictionary with all Attribute Definitions
        /// </summary>
        public IList<IAttributeDefinition> Attributes { get; set; }

        public IAttributeDefinition this[string fieldName] => Attributes.FirstOrDefault(a => a.Name == fieldName);

        #endregion

        public Metadata Metadata = new Metadata();

        #region constructors
        /// <summary>
        /// Initializes a new instance of the ContentType class.
        /// </summary>
        public ContentType(int appId, string name, string staticName, int attributeSetId, string scope, string description, int? usesConfigurationOfAttributeSet, int configZoneId, int configAppId, bool configurationIsOmnipresent)
        {
            AppId = appId;
            Name = name;
            StaticName = staticName;
            ContentTypeId = attributeSetId;
            Description = description;
            Scope = scope;
            ParentConfigurationId = usesConfigurationOfAttributeSet;
            ParentConfigurationZoneId = configZoneId;
            ParentConfigurationAppId = configAppId;
            AlwaysShareConfiguration = configurationIsOmnipresent;
        }

        /// <summary>
        /// Overload for in-memory entities
        /// </summary>
        public ContentType(int appId, string name, string staticName = null)
        {
            AppId = appId;
            Name = name;
            StaticName = staticName ?? name;
        }
        #endregion


        #region Helpers just for creating ContentTypes which will be imported
        public void SetImportParameters(string scope, string staticName, string description, bool alwaysShareDef)
	    {
	        Scope = scope;
	        StaticName = staticName;
	        Description = description;
            AlwaysShareConfiguration = alwaysShareDef;
        }

        // special values just needed for import / save 
        // todo: try to place in a sub-object to un-clutter this ContentType object
        public bool OnSaveSortAttributes { get; set; } = false;
        public string OnSaveUseParentStaticName { get; set; }


        #endregion
    }
}