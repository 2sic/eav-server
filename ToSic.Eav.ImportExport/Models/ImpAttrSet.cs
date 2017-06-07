using System.Collections.Generic;

namespace ToSic.Eav.ImportExport.Models
{
	public class ImpAttrSet
	{
		public string Name { get; set; }
		public string StaticName { get; set; }
		public string Description { get; set; }
		public string Scope { get; set; }
		public List<ImpAttribute> Attributes { get; set; }	// The List<> class guarantees ordering
		public ImpAttribute TitleAttribute { get; set; }
		public bool AlwaysShareConfiguration { get; set; }

	    public string UsesConfigurationOfAttributeSet { get; set; }

	    public bool SortAttributes { get; set; } = false;

        public ImpAttrSet() { }

		private ImpAttrSet(string name, string staticName, string description, string scope, List<ImpAttribute> attributes, bool alwaysShareConfiguration = false)
		{
			Name = name;
			StaticName = staticName;
			Description = description;
			Scope = scope;
			Attributes = attributes;
			AlwaysShareConfiguration = alwaysShareConfiguration;
		}

		/// <summary>
		/// Shortcut go get a new AttributeSet with Scope=System and Name=StaticName
		/// </summary>
		public static ImpAttrSet SystemAttributeSet(string staticName, string description, List<ImpAttribute> attributes, bool alwaysShareConfiguration = false)
		{
			return new ImpAttrSet(staticName, staticName, description, "System", attributes, alwaysShareConfiguration);
		}
	}
}