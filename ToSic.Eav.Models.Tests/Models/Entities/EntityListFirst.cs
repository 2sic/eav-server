using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entities;

public partial class EntityListFirst(MockDataGenerator generator)
{
    [Fact]
    public void FirstNameOfWithSameMetadataNone()
    {
        var entity = generator.CreateEntityWithMetadata(0);
        Null(entity.Metadata.First(nameof(MockMetadataModel)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstNameOfWithSameMetadataMany(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        var md = entity.Metadata.First(nameof(MockMetadataModel));
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.Get<int>(nameof(MockMetadataModel.TargetType)));
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