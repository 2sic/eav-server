using System;

namespace ToSic.Eav.Data.Builder
{
    public class MultiBuilder
    {

        public MultiBuilder(
            Lazy<EntityBuilder> entityBuilder,
            Lazy<AttributeBuilderForImport> attributeBuilder,
            Lazy<ValueBuilder> valueBuilder,
            Lazy<ContentTypeBuilder> contentTypeBuilder)
        {
            _entityBuilder = entityBuilder;
            _attributeBuilder = attributeBuilder;
            _valueBuilder = valueBuilder;
            _contentTypeBuilder = contentTypeBuilder;
        }
        private readonly Lazy<EntityBuilder> _entityBuilder;
        private readonly Lazy<AttributeBuilderForImport> _attributeBuilder;
        private readonly Lazy<ValueBuilder> _valueBuilder;
        private readonly Lazy<ContentTypeBuilder> _contentTypeBuilder;

        public EntityBuilder Entity => _entityBuilder.Value;
        public AttributeBuilderForImport Attribute => _attributeBuilder.Value;

        public ValueBuilder Value => _valueBuilder.Value;

        public ContentTypeBuilder ContentType => _contentTypeBuilder.Value;
    }
}
