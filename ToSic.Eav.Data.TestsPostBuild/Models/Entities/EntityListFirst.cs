using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entities;

public partial class EntityListFirst(TestDataGenerator generator)
{
    [Fact]
    public void FirstNameOfWithSameMetadataNone()
    {
        var entity = generator.CreateEntityWithMetadata(0);
        Null(entity.Metadata.First(nameof(MockModelMetadataForDecorator)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstNameOfWithSameMetadataMany(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        var md = entity.Metadata.First(nameof(MockModelMetadataForDecorator));
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.Get<int>(nameof(MockModelMetadataForDecorator.TargetType)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstNameOfSameMetadataManyTimesNamedWrong(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        Null(entity.Metadata.First(typeName: "some other name"));
    }


}