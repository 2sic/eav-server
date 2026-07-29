using ToSic.Eav.Metadata;
using ToSic.Eav.Models.Factory;
using ToSic.Eav.Models.TestData;
// ReSharper disable InconsistentNaming
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace ToSic.Eav.Models.WithFactory;

public class ToModelRequiringFactory_WithDependencies(TestDataGenerator generator, IModelFactory factory)
    : ToModelWithFactory<MockModelRequiringFactoryWithDependencies>(generator, factory)
{
    [Fact]
    public void ToModel_WithFactory_DependenciesWork()
    {
        var entity = generator.CreateMetadataForDecorator();
        var model = entity.ToModel<MockModelRequiringFactoryWithDependencies>(factory);
        NotNull(model);
        Equal((int)TargetTypes.Entity, model.TargetType);
        Equal(MockModelRequiringFactoryWithDependencies.Dependencies.HelloMessage, model.SomethingFromDependency);
    }

}
public class ToModelRequiringFactory_NoDependencies(TestDataGenerator generator, IModelFactory factory)
    : ToModelWithFactory<MockModelRequiringFactoryNoDependencies>(generator, factory)
{
    [Fact]
    public void ToModel_NoConstructor_RequiredFactoryMissing_Throws() =>
        Throws<InvalidCastException>(() =>
            generator.CreateMetadataForDecorator()
                .ToModelTac<MockModelRequiringFactoryNoDependencies>()
        );
}


public abstract class ToModelWithFactory<TModel>(TestDataGenerator generator, IModelFactory factory)
    where TModel : class, IModelFromEntity
{

    [Fact]
    public void ToModel_WithFactory_Works()
    {
        var entity = generator.CreateMetadataForDecorator();
        var model = entity.ToModel<TModel>(factory);
        NotNull(model);
    }

    [Fact]
    public void ToModel_NullFactory_Throws() =>
        Throws<ArgumentNullException>(() =>
#pragma warning disable IDE0004
                generator.CreateMetadataForDecorator().ToModelTac<TModel>((IModelFactory)null!)
#pragma warning restore IDE0004
        );

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetModel_WithFactory_Works(int amountMdFor)
    {
        var entity = generator.CreateEntityWithMetadata(amountMdFor);
        var list = entity.Metadata.GetModels<TModel>(
            factory,
            typeName: nameof(MockModelMetadataForDecorator)
        );
        Equal(amountMdFor, list.Count());
    }




    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void FirstModel_WithFactory_Works(int amountMdFor)
    {
        var entity = generator.CreateEntityWithMetadata(amountMdFor);
        var first = entity.Metadata.FirstModel<TModel>(factory,
            // TODO: WHAT EXACTLY are we testing here?
            typeName: nameof(MockModelMetadataForDecorator)
        );
        NotNull(first);
    }

}