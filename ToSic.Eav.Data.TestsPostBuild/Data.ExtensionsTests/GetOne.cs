using ToSic.Eav.Data.ExtensionsTests.TestData;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.ExtensionsTests;

public class GetOne(TestDataGenerator generator)
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void WithSameMetadataManyTimes(int amount)
    {
        var entity = generator.EntityWithMdForMetadata(amount);
        var md = entity.Metadata.GetOne<TestModelMetadataForDecorator>();
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void WithSameMetadataManyTimesNamed(int amount)
    {
        var entity = generator.EntityWithMdForMetadata(amount);
        var md = entity.Metadata.GetOne<TestModelMetadataForDecorator>(nameof(TestModelMetadataForDecorator));
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void WithSameMetadataManyTimesNamedWrong(int amount)
    {
        var entity = generator.EntityWithMdForMetadata(amount);
        var md = entity.Metadata.GetOne<TestModelMetadataForDecorator>("some other name");
        Null(md);
    }

}