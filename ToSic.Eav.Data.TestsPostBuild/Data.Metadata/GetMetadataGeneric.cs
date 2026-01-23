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
        var md = entity.Metadata.GetOne<MetadataForDecoratorMock>();
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void WithSameMetadataManyTimesNamed(int amount)
    {
        var entity = generator.EntityWithMdForMetadata(amount);
        var md = entity.Metadata.GetOne<MetadataForDecoratorMock>(nameof(MetadataForDecoratorMock));
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void WithSameMetadataManyTimesNamedWrong(int amount)
    {
        var entity = generator.EntityWithMdForMetadata(amount);
        var md = entity.Metadata.GetOne<MetadataForDecoratorMock>("some other name");
        Null(md);
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
        var mdList = entity.Metadata.GetAll<MetadataForDecoratorMock>();
        NotNull(mdList);
        Equal(amountMdFor, mdList.Count());
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(5, 0)]
    [InlineData(0, 3)]
    [InlineData(2, 4)]
    public void WithMixedMetadataManyTimesNamed(int amountMdFor, int amountOther)
    {
        var entity = generator.CreateWithMixedMetadata(amountMdFor, amountOther);
        var mdList = entity.Metadata.GetAll<MetadataForDecoratorMock>(nameof(MetadataForDecoratorMock));
        NotNull(mdList);
        Equal(amountMdFor, mdList.Count());
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(5, 0)]
    [InlineData(0, 3)]
    [InlineData(2, 4)]
    public void WithMixedMetadataManyTimesNamedWrong(int amountMdFor, int amountOther)
    {
        var entity = generator.CreateWithMixedMetadata(amountMdFor, amountOther);
        var mdList = entity.Metadata.GetAll<MetadataForDecoratorMock>("some-wrong-name");
        NotNull(mdList);
        Empty(mdList);
    }

}