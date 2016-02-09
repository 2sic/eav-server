using System;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Custom Attribute for DataSources and usage in Pipeline Designer
	/// Only DataSources which have this attribute will be listed in the designer-tool
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class PipelineDesignerAttribute : System.Attribute
	{

	}
}