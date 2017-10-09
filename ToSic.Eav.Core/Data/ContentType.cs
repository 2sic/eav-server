using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <inheritdoc cref="IContentType" />
    /// <summary>
    /// Represents a Content Type
    /// </summary>
    public class ContentType : IContentType, IContentTypeShareable
    {
        #region simple properties
        /// <inheritdoc />
        public int AppId { get; }

        /// <inheritdoc />
        public string Name { get; protected set; }

        /// <inheritdoc />
        public string StaticName { get; protected set; }

        /// <inheritdoc />
        public string Description { get; protected set;  }

        /// <inheritdoc />
        public string Scope { get; protected set; }

        /// <inheritdoc />
        public int ContentTypeId { get; }


        /// <inheritdoc />
        public IList<IAttributeDefinition> Attributes { get; set; }

        /// <inheritdoc />
        public IAttributeDefinition this[string fieldName] => Attributes.FirstOrDefault(a => a.Name == fieldName);

        public bool IsInstalledInPrimaryStorage { get; protected set; } = true;

        #endregion

        #region Sharing Content Types

        /// <inheritdoc />
        public int? ParentId { get; }

        /// <inheritdoc />
        public int ParentAppId { get; }

        /// <inheritdoc />
        public int ParentZoneId { get; }

        /// <inheritdoc />
        public bool AlwaysShareConfiguration { get; protected set; }
        #endregion


        //public Metadata Metadata = new Metadata();

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
            ParentId = usesConfigurationOfAttributeSet;
            ParentZoneId = configZoneId;
            ParentAppId = configAppId;
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