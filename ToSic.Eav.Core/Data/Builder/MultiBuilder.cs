using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Builder
{
    public class MultiBuilder: ServiceBase
    {

        public MultiBuilder(
            LazyInit<EntityBuilder> entityBuilder,
            LazyInit<AttributeBuilderForImport> attributeBuilder,
            LazyInit<ValueBuilder> valueBuilder,
            LazyInit<ContentTypeBuilder> contentTypeBuilder): base(LogNames.Eav + "MltBld")
        {
            ConnectServices(
                _entityBuilder = entityBuilder,
                _attributeBuilder = attributeBuilder,
                _valueBuilder = valueBuilder,
                _contentTypeBuilder = contentTypeBuilder
            );
        }
        private readonly LazyInit<EntityBuilder> _entityBuilder;
        private readonly LazyInit<AttributeBuilderForImport> _attributeBuilder;
        private readonly LazyInit<ValueBuilder> _valueBuilder;
        private readonly LazyInit<ContentTypeBuilder> _contentTypeBuilder;

        public EntityBuilder Entity => _entityBuilder.Value;
        public AttributeBuilderForImport Attribute => _attributeBuilder.Value;

        public ValueBuilder Value => _valueBuilder.Value;

        public ContentTypeBuilder ContentType => _contentTypeBuilder.Value;
    }
}
