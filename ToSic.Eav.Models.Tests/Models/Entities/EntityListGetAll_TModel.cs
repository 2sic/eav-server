using ToSic.Eav.Models.TestData;
using ToSic.Eav.Models.WithFactory;

namespace ToSic.Eav.Models.Entities;

public class EntityListGetAll(MockDataGenerator generator)
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
        var mdList = entity.Metadata.GetModels<MockMetadataModel>();
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
        var mdList = entity.Metadata.GetModels<MockMetadataModel>(
            options: new() { TypeName = nameof(MockMetadataModel) });
        NotNull(mdList);
        Equal(amountMdFor, mdList.Count());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void GetAllWithCustomNameIncorrect(int amountMdFor)
    {
        var entity = generator.CreateEntityWithMetadata(amountMdFor);
        var mdList = entity.Metadata
            .GetModels<MockMetadataModel>(options: new() { TypeName = "some-wrong-name" });
        NotNull(mdList);
        Empty(mdList);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void GetAllWithClassNameIncorrect(int amountMdFor)
    {
        var entity = generator.CreateEntityWithMetadata(amountMdFor);
        var mdList = entity.Metadata
            .GetModels<MockMetadataModelWrongName>();
        NotNull(mdList);
        Empty(mdList);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void GetAllWithClassNameIncorrectButName(int amountMdFor)
    {
        var entity = generator.CreateEntityWithMetadata(amountMdFor);
        var mdList = entity.Metadata
            .GetModels<MockMetadataModelWrongName>(options: new() { TypeName = nameof(MockMetadataModel) });
        NotNull(mdList);
        Equal(amountMdFor, mdList.Count());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void GetAllWithClassNameIncorrectButAttribute(int amountMdFor)
    {
        var entity = generator.CreateEntityWithMetadata(amountMdFor);
        var mdList = entity.Metadata
            .GetModels<MockMetadataModelWithSpecsNameRight>();
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
                options: new() { TypeName = nameof(MockMetadataModel) });
        });



}