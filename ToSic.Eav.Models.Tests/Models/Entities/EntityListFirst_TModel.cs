using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entities;

public partial class EntityListFirst
{

    [Fact]
    public void FirstGenericWithSameMetadataNone()
    {
        var entity = generator.CreateEntityWithMetadata(0);
        Null(entity.Metadata.FirstModelTac<MockMetadataModel>());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericWithSameMetadataMany(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        var md = entity.Metadata.FirstModelTac<MockMetadataModel>();
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericSameMetadataManyNamed(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        var md = entity.Metadata.FirstModelTac<MockMetadataModel>(options: new() { TypeName = nameof(MockMetadataModel) });
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericSameMetadataManyTimesNamedWrong(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        Null(entity.Metadata.FirstModelTac<MockMetadataModel>(options: new() { TypeName = "some other name"}));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void NotFoundButForceModel(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        NotNull(entity.Metadata.FirstModelTac<MockMetadataModel>(options: new()
        {
            TypeName = "some other name",
            NullHandling = NullHandling.ReturnModel,
        }));
    }

}
