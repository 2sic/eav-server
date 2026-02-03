using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models;

public partial class EntityListFirst
{

    [Fact]
    public void FirstGenericWithSameMetadataNone()
    {
        var entity = generator.EntityWithMetadataForDecorator(0);
        Null(entity.Metadata.FirstTac<TestModelMetadataForDecorator>());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericWithSameMetadataMany(int amount)
    {
        var entity = generator.EntityWithMetadataForDecorator(amount);
        var md = entity.Metadata.FirstTac<TestModelMetadataForDecorator>();
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericSameMetadataManyNamed(int amount)
    {
        var entity = generator.EntityWithMetadataForDecorator(amount);
        var md = entity.Metadata.FirstTac<TestModelMetadataForDecorator>(typeName: nameof(TestModelMetadataForDecorator));
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void FirstGenericSameMetadataManyTimesNamedWrong(int amount)
    {
        var entity = generator.EntityWithMetadataForDecorator(amount);
        Null(entity.Metadata.FirstTac<TestModelMetadataForDecorator>(typeName: "some other name"));
    }

}
