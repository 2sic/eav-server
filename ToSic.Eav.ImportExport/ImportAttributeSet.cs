using System.Collections.Generic;

namespace ToSic.Eav.Import
{
	public class ImportAttributeSet
	{
		public string Name { get; set; }
		public string StaticName { get; set; }
		public string Description { get; set; }
		public string Scope { get; set; }
		public List<ImportAttribute> Attributes { get; set; }	// The List<> class does guarantee ordering
		public ImportAttribute TitleAttribute { get; set; }
		public bool AlwaysShareConfiguration { get; set; }

	    public string UsesConfigurationOfAttributeSet { get; set; }

	    public bool SortAttributes { get; set; } = false;

        public ImportAttributeSet() { }

		public ImportAttributeSet(string name, string staticName, string description, string scope, List<ImportAttribute> attributes, bool alwaysShareConfiguration = false)
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
		public static ImportAttributeSet SystemAttributeSet(string staticName, string description, List<ImportAttribute> attributes, bool alwaysShareConfiguration = false)
		{
			return new ImportAttributeSet(staticName, staticName, description, "System", attributes, alwaysShareConfiguration);
		}
	}
}