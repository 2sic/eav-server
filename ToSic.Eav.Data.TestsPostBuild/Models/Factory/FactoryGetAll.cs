using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Factory;

public class FactoryGetAll(TestDataGenerator generator, IModelFactory factory)
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetAllRequiringFactory(int amountMdFor)
    {
        var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
        var list = entity.Metadata.GetModels<TestModelRequiringFactoryEmptyConstructor>(
            factory,
            typeName: nameof(TestModelMetadataForDecorator)
        );
        Equal(amountMdFor, list.Count());
    }
}