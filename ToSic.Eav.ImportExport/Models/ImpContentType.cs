using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Models
{
	public class ImpContentType: ContentType
	{
		//public string Name { get; set; }
		//public string StaticName { get; set; }
		//public string Description { get; set; }
		//public string Scope { get; set; }

		public List<ImpAttribDefinition> TempAttribDefinitions { get; set; }	// The List<> class guarantees ordering
		public ImpAttribDefinition TitleAttribute { get; set; }

		//public bool AlwaysShareConfiguration { get; set; }

        
        // Import specific special fields...
	    public string ParentConfigurationStaticName { get; set; }
	    public bool SortAttributes { get; set; } = false;

        public ImpContentType(string name) : base(name) { }

		private ImpContentType(string name, string staticName, string description, string scope, List<ImpAttribDefinition> attributes, bool alwaysShareConfiguration = false)
            : base(name, staticName, 0, scope, description, null, 0, 0, alwaysShareConfiguration)
		{
			//Name = name;
			//StaticName = staticName;
			//Description = description;
			//Scope = scope;
			TempAttribDefinitions = attributes;
			//AlwaysShareConfiguration = alwaysShareConfiguration;
		}

		/// <summary>
		/// Shortcut go get a new AttributeSet with Scope=System and Name=StaticName
		/// </summary>
		public static ImpContentType SystemAttributeSet(string staticName, string description, List<ImpAttribDefinition> attributes, bool alwaysShareConfiguration = false)
		{
			return new ImpContentType(staticName, staticName, description, "System", attributes, alwaysShareConfiguration);
		}

	    public void SetImportParameters(string scope, bool alwaysShareDef)
	    {
	        AlwaysShareConfiguration = alwaysShareDef;
	        Scope = scope;
	    }
	}
}