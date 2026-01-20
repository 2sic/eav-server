using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Data.Metadata;

public class GetMetadataGeneric(EntityWithMetadataGenerator generator)
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void WithSameMetadataManyTimes(int amount)
    {
        var entity = generator.EntityWithMdForMetadata(amount);
        var md = entity.GetMetadata<MetadataForDecorator>();
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }


    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(5, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 3)]
    [InlineData(2, 4)]
    public void WithMixedMetadataManyTimes(int amountMdFor, int amountOther)
    {
        var entity = generator.CreateWithMixedMetadata(amountMdFor, amountOther);
        var mdList = entity.GetMetadata<List<MetadataForDecorator>, MetadataForDecorator>();
        NotNull(mdList);
        Equal(amountMdFor /*+ amountOther*/, mdList.Count());
    }

}