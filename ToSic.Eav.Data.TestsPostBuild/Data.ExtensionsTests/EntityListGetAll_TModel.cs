using ToSic.Eav.Data.ExtensionsTests.TestData;

namespace ToSic.Eav.Data.ExtensionsTests;

public class EntityListGetAll(TestDataGenerator generator)
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
        var mdList = entity.Metadata.GetAll<TestModelMetadataForDecorator>(typeName: nameof(TestModelMetadataForDecorator));
        NotNull(mdList);
        Equal(amountMdFor, mdList.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(2)]
    public void GetAllWithCustomNameIncorrect(int amountMdFor)
    {
        var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
        var mdList = entity.Metadata
            .GetAll<TestModelMetadataForDecorator>(typeName: "some-wrong-name");
        NotNull(mdList);
        Empty(mdList);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(2)]
    public void GetAllWithClassNameIncorrect(int amountMdFor)
    {
        var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
        var mdList = entity.Metadata
            .GetAll<TestModelMetadataForDecoratorWrongName>();
        NotNull(mdList);
        Empty(mdList);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(2)]
    public void GetAllWithClassNameIncorrectButName(int amountMdFor)
    {
        var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
        var mdList = entity.Metadata
            .GetAll<TestModelMetadataForDecoratorWrongName>(typeName: nameof(TestModelMetadataForDecorator));
        NotNull(mdList);
        Equal(amountMdFor, mdList.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(2)]
    public void GetAllWithClassNameIncorrectButAttribute(int amountMdFor)
    {
        var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
        var mdList = entity.Metadata
            .GetAll<TestModelMetadataForDecoratorWithAttribute>();
        NotNull(mdList);
        Equal(amountMdFor, mdList.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetAllRequiringFactoryMissingFails(int amountMdFor) =>
        Throws<InvalidCastException>(() =>
        {
            var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
            entity.Metadata.GetAll<TestModelRequiringFactoryEmptyConstructor>(
                typeName: nameof(TestModelMetadataForDecorator));
        });



}