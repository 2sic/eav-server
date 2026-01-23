using ToSic.Eav.Data.ExtensionsTests.TestData;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.ExtensionsTests;

public class As(TestDataGenerator generator)
{
    [Fact]
    public void BasicCast()
    {
        var entity = generator.CreateMetadataForDecorator();
        var converted = entity.As<TestModelMetadataForDecorator>();
        NotNull(converted);
        Equal((int)TargetTypes.Entity, converted.TargetType);
    }


}