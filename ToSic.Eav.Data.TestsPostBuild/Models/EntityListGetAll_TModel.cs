using ToSic.Eav.Data;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models;

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
        var mdList = entity.Metadata.GetModels<TestModelMetadataForDecorator>();
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
        var mdList = entity.Metadata.GetModels<TestModelMetadataForDecorator>(typeName: nameof(TestModelMetadataForDecorator));
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
            .GetModels<TestModelMetadataForDecorator>(typeName: "some-wrong-name");
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
            .GetModels<TestModelMetadataForDecoratorWrongName>();
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
            .GetModels<TestModelMetadataForDecoratorWrongName>(typeName: nameof(TestModelMetadataForDecorator));
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
            .GetModels<TestModelMetadataForDecoratorWithAttribute>();
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
            entity.Metadata.GetModels<TestModelRequiringFactoryEmptyConstructor>(
                typeName: nameof(TestModelMetadataForDecorator));
        });



}