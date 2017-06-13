using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data.Builder
{
    public static class ContentType
    {
		/// <summary>
		/// Shortcut go get a new AttributeSet with Scope=System and Name=StaticName
		/// </summary>
		public static Data.ContentType SystemAttributeSet(string staticName, string description, List<IAttributeDefinition> attributes, bool alwaysShareConfiguration = false)
		{
            return new Data.ContentType(staticName, staticName, 0, "System", description, null, 0, 0, alwaysShareConfiguration)
            {
               	Attributes = attributes
            };
		}
    }
}
