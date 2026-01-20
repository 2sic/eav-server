using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.TestData;

namespace ToSic.Eav.Data.Metadata;

public class EntityWithMetadataGenerator(DataBuilder builder, ContentTypeFactory ctFactory)
{
    public IEntity EntityWithMdForMetadata(int amount)
    {
        var original = builder.TestEntityDaniel();

        var decorators = CreateMdForDecorators(amount);

        var lazyPartsBuilder = builder.EntityConnection.UseMetadata(decorators);

        var entity = builder.Entity.CreateFrom(original, partsBuilder: lazyPartsBuilder);
        return entity;
    }

    public IEntity CreateWithMixedMetadata(int amountMdFor, int amountOther)
    {
        var original = builder.TestEntityDaniel();

        var decorators = CreateMdForDecorators(amountMdFor);

        var newDecorators = CreateMdEmpty(amountOther);

        var lazyPartsBuilder = builder.EntityConnection.UseMetadata(decorators.Concat(newDecorators));

        var entity = builder.Entity.CreateFrom(original, partsBuilder: lazyPartsBuilder);
        return entity;
    }

    public IEnumerable<IEntity> CreateMdEmpty(int amountOther)
    {
        var mdEmptyDecorator = MetadataSamples.CreateEntityForNoSpecs(builder, ctFactory);
        var newDecorators = Enumerable
            .Range(1, amountOther)
            .Select(i => mdEmptyDecorator);
        return newDecorators;
    }

    public IEnumerable<IEntity> CreateMdForDecorators(int amount)
    {
        var mdForDecorator = MetadataSamples.CreateMetadataForDecorator(builder, ctFactory);
        var decorators = Enumerable
            .Range(1, amount)
            .Select(i => mdForDecorator);
        return decorators;
    }
}
