using ToSic.Eav.Data.ExtensionsTests.TestData;
using ToSic.Eav.Metadata;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.ExtensionsTests;

public class As(TestDataGenerator generator, IWrapperFactory factory)
{
    [Fact]
    public void BasicAs()
    {
        var entity = generator.CreateMetadataForDecorator();
        var model = entity.As<TestModelMetadataForDecorator>();
        NotNull(model);
        Equal((int)TargetTypes.Entity, model.TargetType);
    }

    [Fact]
    public void BasicAsRequiresFactoryFails() =>
        Throws<InvalidCastException>(() =>
        {
            var entity = generator.CreateMetadataForDecorator();
            return entity.As<TestModelRequiringFactoryEmptyConstructor>();
        });

    [Fact]
    public void AsWithFactory()
    {
        var entity = generator.CreateMetadataForDecorator();
        var model = entity.As<TestModelRequiringFactory>(factory);
        NotNull(model);
        Equal((int)TargetTypes.Entity, model.TargetType);
        Equal(TestModelDependency.HelloMessage, model.SomethingFromDependency);
    }
}