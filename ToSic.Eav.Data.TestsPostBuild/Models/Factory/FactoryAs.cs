using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Factory;

public class FactoryAs(TestDataGenerator generator, IModelFactory factory)
{

    [Fact]
    public void AssembleWithFactory()
    {
        var entity = generator.CreateMetadataForDecorator();
        var model = factory.As<TestModelRequiringFactory>(entity);
        NotNull(model);
        Equal((int)TargetTypes.Entity, model.TargetType);
        Equal(TestModelRequiringFactory.TestModelDependencyInjection.HelloMessage, model.SomethingFromDependency);
    }

    [Fact]
    public void AssembleWithFactoryNullThrows() =>
        Throws<ArgumentNullException>(() =>
            ((IModelFactory)null).As<TestModelRequiringFactory>(generator.CreateMetadataForDecorator())
        );
}