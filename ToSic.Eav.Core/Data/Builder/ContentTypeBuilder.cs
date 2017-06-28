using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data.Builder
{
    public static class ContentTypeBuilder
    {
		/// <summary>
		/// Shortcut go get a new AttributeSet with Scope=System and Name=StaticName
		/// </summary>
		public static ContentType SystemAttributeSet(int appId, string staticName, string description, List<IAttributeDefinition> attributes, bool alwaysShareConfiguration = false)
		{
            return new ContentType(appId, staticName, staticName, 0, "System", description, null, 0, 0, alwaysShareConfiguration)
            {
               	Attributes = attributes
            };
		}

        //public static ContentType UndefinedContentTypeType() => new ContentType("");
    }
}
