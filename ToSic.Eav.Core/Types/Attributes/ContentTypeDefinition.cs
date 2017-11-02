using System;

namespace ToSic.Eav.Types.Attributes
{
	/// <inheritdoc />
	/// <summary>
	/// Custom Attribute for DataSources and usage in Pipeline Designer
	/// Only DataSources which have this attribute will be listed in the designer-tool
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ContentTypeDefinition : Attribute
	{
        public string StaticName { get;  }

	    public ContentTypeDefinition(string staticName)
	    {
	        StaticName = staticName;
	    }

	}
}