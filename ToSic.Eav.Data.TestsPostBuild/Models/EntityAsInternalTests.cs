using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models;

public class EntityAsInternalTests(TestDataGenerator generator)
{
    [Fact]
    public void AsInternalBasic()
    {
        var entity = generator.CreateMetadataForDecorator();
        var model = entity.AsInternalTac<TestModelMetadataForDecorator>();
        NotNull(model);
        Equal((int)TargetTypes.Entity, model.TargetType);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record WithConstructor(string something) : TestModelMetadataForDecorator;

    [Fact]
    public void ThrowsWithoutEmptyConstructor() =>
        Throws<InvalidOperationException>(() =>
        {
            var entity = generator.CreateMetadataForDecorator();
            entity.AsInternalTac<WithConstructor>(skipTypeCheck: true);
        });
}
