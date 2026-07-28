using ToSic.Eav.Models.TestData;
using ToSic.Eav.Models.WithFactory;

namespace ToSic.Eav.Models.Entities;

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
        var entity = generator.CreateEntityWithMetadata(amountMdFor, amountOther);
        var mdList = entity.Metadata.GetModels<MockModelMetadataForDecorator>();
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
        var entity = generator.CreateEntityWithMetadata(amountMdFor, amountOther);
        var mdList = entity.Metadata.GetModels<MockModelMetadataForDecorator>(typeName: nameof(MockModelMetadataForDecorator));
        NotNull(mdList);
        Equal(amountMdFor, mdList.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(2)]
    public void GetAllWithCustomNameIncorrect(int amountMdFor)
    {
        var entity = generator.CreateEntityWithMetadata(amountMdFor);
        var mdList = entity.Metadata
            .GetModels<MockModelMetadataForDecorator>(typeName: "some-wrong-name");
        NotNull(mdList);
        Empty(mdList);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(2)]
    public void GetAllWithClassNameIncorrect(int amountMdFor)
    {
        var entity = generator.CreateEntityWithMetadata(amountMdFor);
        var mdList = entity.Metadata
            .GetModels<MockModelMetadataForDecoratorWrongName>();
        NotNull(mdList);
        Empty(mdList);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(2)]
    public void GetAllWithClassNameIncorrectButName(int amountMdFor)
    {
        var entity = generator.CreateEntityWithMetadata(amountMdFor);
        var mdList = entity.Metadata
            .GetModels<MockModelMetadataForDecoratorWrongName>(typeName: nameof(MockModelMetadataForDecorator));
        NotNull(mdList);
        Equal(amountMdFor, mdList.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(2)]
    public void GetAllWithClassNameIncorrectButAttribute(int amountMdFor)
    {
        var entity = generator.CreateEntityWithMetadata(amountMdFor);
        var mdList = entity.Metadata
            .GetModels<MockModelMetadataForDecoratorWithModelSpecs>();
        NotNull(mdList);
        Equal(amountMdFor, mdList.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetAllRequiringFactoryMissingFails(int amountMdFor) =>
        Throws<InvalidCastException>(() =>
        {
            var entity = generator.CreateEntityWithMetadata(amountMdFor);
            entity.Metadata.GetModels<MockModelRequiringFactoryNoDependencies>(
                typeName: nameof(MockModelMetadataForDecorator));
        });



}