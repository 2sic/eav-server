using System;

namespace ToSic.Eav.Data.Beta_TypeSystem
{
	/// <inheritdoc />
	/// <summary>
	/// Custom Attribute for DataSources and usage in Pipeline Designer
	/// Only DataSources which have this attribute will be listed in the designer-tool
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ContentTypeDefinition : Attribute
	{
	    // ReSharper disable once InconsistentNaming
	    public ContentTypeDefinition(string StaticName)
	    {
	        _staticName = StaticName;
	    }

	    private string _staticName;

	}
}