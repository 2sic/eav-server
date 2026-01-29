using ToSic.Eav.Data.ExtensionsTests.TestData;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.ExtensionsTests;

public partial class EntityListFirst
{

    [Fact]
    public void FirstGenericWithSameMetadataNone()
    {
        var entity = generator.EntityWithMetadataForDecorator(0);
        Null(entity.Metadata.First<TestModelMetadataForDecorator>());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericWithSameMetadataMany(int amount)
    {
        var entity = generator.EntityWithMetadataForDecorator(amount);
        var md = entity.Metadata.First<TestModelMetadataForDecorator>();
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericSameMetadataManyNamed(int amount)
    {
        var entity = generator.EntityWithMetadataForDecorator(amount);
        var md = entity.Metadata.First<TestModelMetadataForDecorator>(typeName: nameof(TestModelMetadataForDecorator));
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericSameMetadataManyTimesNamedWrong(int amount)
    {
        var entity = generator.EntityWithMetadataForDecorator(amount);
        Null(entity.Metadata.First<TestModelMetadataForDecorator>(typeName: "some other name"));
    }

}