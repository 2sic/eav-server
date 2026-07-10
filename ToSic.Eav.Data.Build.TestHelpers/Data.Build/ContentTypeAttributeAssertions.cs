using ToSic.Eav.Data.Sys.Attributes;

namespace ToSic.Eav.Data.Build;

public static class ContentTypeAttributeAssertions
{
    public static void AssertAttribute(this IContentType ct, string name, ValueTypes type, bool isTitle = false, string? description = default)
    {
        var attribute = ct.Attributes.FirstOrDefault(a => a.Name == name);
        NotNull(attribute);
        attribute
            .HasName(name)
            .IsType(type)
            .IsTitle(isTitle)
            .HasDescription(description);
    }

    extension(IContentTypeAttribute attr)
    {
        public IContentTypeAttribute HasName(string name)
        {
            Equal(name, attr.Name);
            return attr;
        }

        public IContentTypeAttribute IsType(ValueTypes type)
        {
            Equal(type, attr.Type);
            return attr;
        }

        public IContentTypeAttribute IsTitle(bool isTitle = false)
        {
            Equal(isTitle, attr.IsTitle);
            return attr;
        }
        
        public IContentTypeAttribute HasDescription(string? description = null)
        {
            Equal(description, attr.Metadata.Get<string>(AttributeMetadataConstants.DescriptionField));
            return attr;
        }
    }
}
