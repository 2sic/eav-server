using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.TestData;

namespace ToSic.Eav.Data.Metadata;

public class EntityWithMetadata(DataBuilder builder, ContentTypeFactory ctFactory)
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void TestMethod1(int amount)
    {
        var entity = CreateEntityWithMetadata_MetadataForDecorator(amount);
        Equal(amount, entity.Metadata.Count());
    }

    public IEntity CreateEntityWithMetadata_MetadataForDecorator(int amount)
    {
        var entity = builder.TestEntityDaniel();

        var mdForDecorator = MetadataSamples.CreateMetadataForDecorator(builder, ctFactory);
        var decorators = Enumerable.Range(1, amount).Select(i => mdForDecorator);

        var lazyPartsBuilder = builder.EntityConnection.UseMetadata(decorators);

        var merged = builder.Entity.CreateFrom(entity, partsBuilder: lazyPartsBuilder);
        return merged;
    }
}