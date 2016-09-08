using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Content Type
    /// </summary>
    public class ContentType : IContentType
    {
        #region simple properties
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Static name - can be a GUID or a system-term for special types
        /// </summary>
        public string StaticName { get; set; }

        public string Description { get; set;  }

        /// <summary>
        /// Internal Id of the attribute-set of this content-type. Don't worry about this one, you probably won't understand it and that's ok. 
        /// </summary>
        public int AttributeSetId { get; private set; }
        /// <summary>
        /// What this content-types if for, if it's a system type or something
        /// </summary>
        public string Scope { get; private set; }
        /// <summary>
        /// todo
        /// </summary>
        public int? UsesConfigurationOfAttributeSet { get; private set; }
        public int ConfigurationAppId { get; private set; }
        public int ConfigurationZoneId { get; private set; }

        public bool ConfigurationIsOmnipresent { get; private set; }

        /// <summary>
		/// Dictionary with all Attribute Definitions
        /// </summary>
        public IDictionary<int, IAttributeBase> AttributeDefinitions { get; set; }

        public IAttributeBase this[string fieldName]
        {
            get {
                var found = AttributeDefinitions.Where(a => a.Value.Name == fieldName).Select(x => x.Value).FirstOrDefault();
                return found; 
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the ContentType class.
        /// </summary>
        public ContentType(string name, string staticName, int attributeSetId, string scope, string description, int? usesConfigurationOfAttributeSet, int configZoneId, int configAppId, bool configurationIsOmnipresent)
        {
            Name = name;
            StaticName = staticName;
            AttributeSetId = attributeSetId;
            Description = description;
            Scope = scope;
            UsesConfigurationOfAttributeSet = usesConfigurationOfAttributeSet;
            ConfigurationZoneId = configZoneId;
            ConfigurationAppId = configAppId;
            ConfigurationIsOmnipresent = configurationIsOmnipresent;
        }

        /// <summary>
        /// Overload for in-memory entities
        /// </summary>
        /// <param name="name"></param>
        public ContentType(string name)
        {
            Name = name;
        }

    }
}