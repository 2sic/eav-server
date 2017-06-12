using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Models
{
	public class ImpContentType: ContentType
	{
		public List<ImpAttribDefinition> TempAttribDefinitions { get; set; }	// The List<> class guarantees ordering
		//public ImpAttribDefinition TitleAttribute { get; set; }

        // Import specific special fields...
	    public string ParentConfigurationStaticName { get; set; }
	    public bool SortAttributes { get; set; } = false;

        public ImpContentType(string name) : base(name) { }

		private ImpContentType(string name, string staticName, string description, string scope, List<ImpAttribDefinition> attributes, bool alwaysShareConfiguration = false)
            : base(name, staticName, 0, scope, description, null, 0, 0, alwaysShareConfiguration)
		{
			TempAttribDefinitions = attributes;
		}

		/// <summary>
		/// Shortcut go get a new AttributeSet with Scope=System and Name=StaticName
		/// </summary>
		public static ImpContentType SystemAttributeSet(string staticName, string description, List<ImpAttribDefinition> attributes, bool alwaysShareConfiguration = false)
		{
			return new ImpContentType(staticName, staticName, description, "System", attributes, alwaysShareConfiguration);
		}

	    public void SetImportParameters(string scope, string staticName, string description, bool alwaysShareDef)
	    {
	        Scope = scope;
	        StaticName = staticName;
	        Description = description;
            AlwaysShareConfiguration = alwaysShareDef;
        }
    }
}