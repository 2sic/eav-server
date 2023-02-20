using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Builder
{
    public class MultiBuilder: ServiceBase
    {

        public MultiBuilder(
            LazySvc<EntityBuilder> entityBuilder,
            LazySvc<AttributeBuilderForImport> attributeBuilder,
            LazySvc<ValueBuilder> valueBuilder,
            LazySvc<ContentTypeBuilder> contentTypeBuilder): base(EavLogs.Eav + "MltBld")
        {
            ConnectServices(
                _entityBuilder = entityBuilder,
                _attributeBuilder = attributeBuilder,
                _valueBuilder = valueBuilder,
                _contentTypeBuilder = contentTypeBuilder
            );
        }
        private readonly LazySvc<EntityBuilder> _entityBuilder;
        private readonly LazySvc<AttributeBuilderForImport> _attributeBuilder;
        private readonly LazySvc<ValueBuilder> _valueBuilder;
        private readonly LazySvc<ContentTypeBuilder> _contentTypeBuilder;

        public EntityBuilder Entity => _entityBuilder.Value;
        public AttributeBuilderForImport Attribute => _attributeBuilder.Value;

        public ValueBuilder Value => _valueBuilder.Value;

        public ContentTypeBuilder ContentType => _contentTypeBuilder.Value;

        /// <summary>
        /// Does a full-clone while also cloning (separating) attributes and relationships...?
        /// Note that relationships are not 100% clear if it's a full clone
        /// ATM only used by the tree builder which generate ephemeral data
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Entity FullClone(IEntity entity)
        {
            return Entity.Clone(entity,
                Attribute.Clone(entity.Attributes),
                ((RelationshipManager)entity.Relationships).AllRelationships);
        }
    }
}
