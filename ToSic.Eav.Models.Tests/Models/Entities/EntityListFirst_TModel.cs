using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entities;

public partial class EntityListFirst
{

    [Fact]
    public void FirstGenericWithSameMetadataNone()
    {
        var entity = generator.CreateEntityWithMetadata(0);
        Null(entity.Metadata.FirstModelTac<MockModelMetadataForDecorator>());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericWithSameMetadataMany(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        var md = entity.Metadata.FirstModelTac<MockModelMetadataForDecorator>();
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericSameMetadataManyNamed(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        var md = entity.Metadata.FirstModelTac<MockModelMetadataForDecorator>(options: new() { TypeName = nameof(MockModelMetadataForDecorator) });
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericSameMetadataManyTimesNamedWrong(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        Null(entity.Metadata.FirstModelTac<MockModelMetadataForDecorator>(options: new() { TypeName = "some other name"}));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void NotFoundButForceModel(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        NotNull(entity.Metadata.FirstModelTac<MockModelMetadataForDecorator>(options: new()
        {
            TypeName = "some other name",
            NullHandling = ToModelOptions.DataNullHandling.ConvertForce,
        }));
    }

}
