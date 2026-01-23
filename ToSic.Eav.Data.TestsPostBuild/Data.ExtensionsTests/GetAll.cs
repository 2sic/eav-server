using ToSic.Eav.Data.ExtensionsTests.TestData;

namespace ToSic.Eav.Data.ExtensionsTests;

public class GetAll(TestDataGenerator generator)
{


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
        var mdList = entity.Metadata.GetAll<TestModelMetadataForDecorator>();
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
        var mdList = entity.Metadata.GetAll<TestModelMetadataForDecorator>(nameof(TestModelMetadataForDecorator));
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
        var mdList = entity.Metadata.GetAll<TestModelMetadataForDecorator>("some-wrong-name");
        NotNull(mdList);
        Empty(mdList);
    }

}