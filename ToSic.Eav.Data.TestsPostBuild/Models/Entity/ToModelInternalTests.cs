using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entity;

public class ToModelInternalTests(TestDataGenerator generator)
{
    [Fact]
    public void Simple_Works()
    {
        var entity = generator.CreateMetadataForDecorator();
        var model = entity.ToModelInternalTac<MockModelMetadataForDecorator>();
        NotNull(model);
        Equal((int)TargetTypes.Entity, model.TargetType);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record WithConstructor(string Something) : IModelFromEntity;

    [Fact]
    public void WithConstructor_Throws() =>
        Throws<InvalidOperationException>(() =>
        {
            var entity = generator.CreateMetadataForDecorator();
            entity.ToModelInternalTac<WithConstructor>(skipTypeCheck: true);
        });

    [Fact]
    public void FromInterface_Works()
    {
        var entity = generator.CreateMetadataForDecorator();
        var model = entity.ToModelInternalTac<IMockModelMetadataForDecorator>(skipTypeCheck: true);
        NotNull(model);
        IsType<MockModelMetadataForDecorator>(model);
    }
}
