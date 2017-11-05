using System;

namespace ToSic.Eav.DataSources.Attributes
{
	/// <inheritdoc />
	/// <summary>
	/// Custom Attribute for DataSources and usage in Pipeline Designer
	/// Only DataSources which have this attribute will be listed in the designer-tool
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class DataSourceProperties : Attribute
	{
	    public DataSourceType Type { get; set; }

        public string Icon { get; set; }

        public string[] In { get; set; }
	}
}