using ToSic.Eav.Data.ExtensionsTests.TestData;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.ExtensionsTests;

public partial class EntityListFirst(TestDataGenerator generator)
{
    [Fact]
    public void FirstNameOfWithSameMetadataNone()
    {
        var entity = generator.EntityWithMetadataForDecorator(0);
        Null(entity.Metadata.First(nameof(TestModelMetadataForDecorator)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstNameOfWithSameMetadataMany(int amount)
    {
        var entity = generator.EntityWithMetadataForDecorator(amount);
        var md = entity.Metadata.First(nameof(TestModelMetadataForDecorator));
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.Get<int>(nameof(TestModelMetadataForDecorator.TargetType)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstNameOfSameMetadataManyTimesNamedWrong(int amount)
    {
        var entity = generator.EntityWithMetadataForDecorator(amount);
        Null(entity.Metadata.First(typeName: "some other name"));
    }


}