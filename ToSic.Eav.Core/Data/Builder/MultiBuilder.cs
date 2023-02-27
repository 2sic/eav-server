using System.Collections.Generic;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Builder
{
    public class MultiBuilder: ServiceBase
    {
        private readonly LazySvc<AttributeBuilder> _attributeBuilder;

        public MultiBuilder(
            LazySvc<EntityBuilder> entityBuilder,
            LazySvc<AttributeBuilderForImport> attributeImport,
            LazySvc<AttributeBuilder> attributeBuilder,
            LazySvc<ValueBuilder> valueBuilder,
            LazySvc<ContentTypeBuilder> contentTypeBuilder): base(EavLogs.Eav + "MltBld")
        {
            ConnectServices(
                _entityBuilder = entityBuilder,
                _attributeImport = attributeImport,
                _contentTypeBuilder = contentTypeBuilder,
                _attributeBuilder = attributeBuilder,
                _valueBuilder = valueBuilder
            );
        }
        private readonly LazySvc<EntityBuilder> _entityBuilder;
        private readonly LazySvc<AttributeBuilderForImport> _attributeImport;
        private readonly LazySvc<ValueBuilder> _valueBuilder;
        private readonly LazySvc<ContentTypeBuilder> _contentTypeBuilder;

        public ContentTypeBuilder ContentType => _contentTypeBuilder.Value;
        public EntityBuilder Entity => _entityBuilder.Value;

        public AttributeBuilder Attribute => _attributeBuilder.Value;
        public AttributeBuilderForImport AttributeImport => _attributeImport.Value;

        public ValueBuilder Value => _valueBuilder.Value;


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
                values: Attribute.ListDeepClone(entity.Attributes));
        }
        

        public IEntity FakeEntity(int appId)
            => _entityBuilder.Value.Create(
                appId: appId,
                rawValues: new Dictionary<string, object> { { Data.Attributes.TitleNiceName, "" } },
                contentType: ContentType.Transient("FakeEntity"),
                titleField: Data.Attributes.TitleNiceName
            );

    }
}
