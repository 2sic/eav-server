using System.Collections.Generic;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build
{
    public class DataBuilder: ServiceBase
    {

        public DataBuilder(
            LazySvc<EntityBuilder> entityBuilder,
            LazySvc<AttributeBuilder> attributeBuilder,
            LazySvc<ValueBuilder> valueBuilder,
            LazySvc<ContentTypeBuilder> contentTypeBuilder,
            LazySvc<ContentTypeAttributeBuilder> typeAttributeBuilder,
            LazySvc<DimensionBuilder> languageBuilder): base(EavLogs.Eav + "MltBld")
        {
            ConnectServices(
                _entityBuilder = entityBuilder,
                _contentTypeBuilder = contentTypeBuilder,
                _attributeBuilder = attributeBuilder,
                _valueBuilder = valueBuilder,
                _typeAttributeBuilder = typeAttributeBuilder,
                _languageBuilder = languageBuilder
            );
        }
        private readonly LazySvc<DimensionBuilder> _languageBuilder;
        private readonly LazySvc<ContentTypeAttributeBuilder> _typeAttributeBuilder;
        private readonly LazySvc<AttributeBuilder> _attributeBuilder;
        private readonly LazySvc<EntityBuilder> _entityBuilder;
        private readonly LazySvc<ValueBuilder> _valueBuilder;
        private readonly LazySvc<ContentTypeBuilder> _contentTypeBuilder;

        public ContentTypeBuilder ContentType => _contentTypeBuilder.Value;
        public EntityBuilder Entity => _entityBuilder.Value;

        public AttributeBuilder Attribute => _attributeBuilder.Value;

        public ValueBuilder Value => _valueBuilder.Value;

        public ContentTypeAttributeBuilder TypeAttributeBuilder => _typeAttributeBuilder.Value;

        public DimensionBuilder Language => _languageBuilder.Value;

        /// <summary>
        /// Does a full-clone while also cloning (separating) attributes and relationships...?
        /// Note that relationships are not 100% clear if it's a full clone
        /// ATM only used by the tree builder which generate ephemeral data
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        // TODO: PROBABLY OBSOLETE
        public IEntity FullClone(IEntity entity)
        {
            return Entity.Clone(entity);
        }
        

        public IEntity FakeEntity(int appId)
            => _entityBuilder.Value.Create(
                appId: appId,
                attributes: Attribute.Create(new Dictionary<string, object> { { Attributes.TitleNiceName, "" } }),
                contentType: ContentType.Transient("FakeEntity"),
                titleField: Attributes.TitleNiceName
            );



    }
}
