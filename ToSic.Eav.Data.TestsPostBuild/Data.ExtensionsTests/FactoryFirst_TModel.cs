using ToSic.Eav.Data.ExtensionsTests.TestData;
using ToSic.Eav.Metadata;
using ToSic.Eav.Models.Factory;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.ExtensionsTests;

public partial class FactoryFirst_TModel(TestDataGenerator generator, IModelFactory factory)
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetOneRequiringFactory(int amountMdFor)
    {
        var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
        factory.First<TestModelRequiringFactoryEmptyConstructor>(entity.Metadata,
            typeName: nameof(TestModelMetadataForDecorator)
        );
    }

}