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

	    // ReSharper disable once InconsistentNaming - required to ensure it's large case and still required in the attribute itself
	    public ContentTypeDefinition(string StaticName)
	    {
	        this.StaticName = StaticName;
	    }

	    //private string _staticName;

	}
}