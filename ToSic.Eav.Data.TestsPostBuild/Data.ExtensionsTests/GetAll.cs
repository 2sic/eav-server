using ToSic.Eav.Data.ExtensionsTests.TestData;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.ExtensionsTests;

public class GetAll(TestDataGenerator generator, IWrapperFactory factory)
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
    public void GetAllWithNameMixed(int amountMdFor, int amountOther)
    {
        var entity = generator.CreateWithMixedMetadata(amountMdFor, amountOther);
        var mdList = entity.Metadata.GetAll<TestModelMetadataForDecorator>(nameof(TestModelMetadataForDecorator));
        NotNull(mdList);
        Equal(amountMdFor, mdList.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(2)]
    public void GetAllWithNameIncorrect(int amountMdFor)
    {
        var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
        var mdList = entity.Metadata.GetAll<TestModelMetadataForDecorator>("some-wrong-name");
        NotNull(mdList);
        Empty(mdList);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetAllRequiringFactoryMissingFails(int amountMdFor) =>
        Throws<InvalidCastException>(() =>
        {
            var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
            entity.Metadata.GetAll<TestModelRequiringFactoryEmptyConstructor>(
                nameof(TestModelMetadataForDecorator));
        });

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetAllRequiringFactory(int amountMdFor)
    {
        var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
        var list = factory.GetAll<TestModelRequiringFactoryEmptyConstructor>(
            entity.Metadata,
            typeName: nameof(TestModelMetadataForDecorator)
        );
        Equal(amountMdFor, list.Count());
    }

}