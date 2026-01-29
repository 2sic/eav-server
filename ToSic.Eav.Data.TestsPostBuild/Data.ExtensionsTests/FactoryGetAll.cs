using ToSic.Eav.Data.ExtensionsTests.TestData;
using ToSic.Eav.Models.Factory;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.ExtensionsTests;

public class FactoryGetAll(TestDataGenerator generator, IModelFactory factory)
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetAllRequiringFactory(int amountMdFor)
    {
        var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
        var list = factory.GetAll<TestModelRequiringFactoryEmptyConstructor>(
            entity.Metadata,
            typeName: nameof(TestModelMetadataForDecorator)
        );
        Equal(amountMdFor, list.Count());
    }
}