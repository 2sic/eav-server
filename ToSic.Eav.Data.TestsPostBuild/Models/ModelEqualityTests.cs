using ToSic.Eav.Data;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models;

public class ModelEqualityTests(TestDataGenerator generator)
{
    private static void CheckAllEquals(TestModelMetadataForDecorator md, TestModelMetadataForDecorator md2)
    {
        Equal(md, md2);
        True(md == md2);
        True(md.Equals(md2));
    }

    [Fact]
    public void EqualsSelf()
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.As<TestModelMetadataForDecorator>()!;
        CheckAllEquals(md, md);
    }


    [Fact]
    public void EqualsSecondCast()
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.As<TestModelMetadataForDecorator>()!;
        var md2 = entity.As<TestModelMetadataForDecorator>()!;
        CheckAllEquals(md, md2);
    }

    [Fact]
    public void EqualsRecast_UsingAsICanBeEntity()
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.As<TestModelMetadataForDecorator>()!;
        // Recast via ICanBeEntity
        var md2 = md.As<TestModelMetadataForDecorator>()!;
        CheckAllEquals(md, md2);
    }

    [Fact]
    public void EqualsRecordWith()
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.As<TestModelMetadataForDecorator>()!;
        var md2 = md with { };
        CheckAllEquals(md, md2);
    }


}
