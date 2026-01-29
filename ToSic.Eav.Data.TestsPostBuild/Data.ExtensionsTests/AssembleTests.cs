using ToSic.Eav.Data.ExtensionsTests.TestData;
using ToSic.Eav.Metadata;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.ExtensionsTests;

public class AssembleTests(TestDataGenerator generator, IWrapperFactory factory)
{

    [Fact]
    public void AssembleWithFactory()
    {
        var entity = generator.CreateMetadataForDecorator();
        var model = factory.As<TestModelRequiringFactory>(entity);
        NotNull(model);
        Equal((int)TargetTypes.Entity, model.TargetType);
        Equal(TestModelDependency.HelloMessage, model.SomethingFromDependency);
    }

    [Fact]
    public void AssembleWithFactoryNullThrows() =>
        Throws<ArgumentNullException>(() =>
            ((IWrapperFactory)null).As<TestModelRequiringFactory>(generator.CreateMetadataForDecorator())
        );
}