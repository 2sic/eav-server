using ToSic.Eav.Data;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Factory;

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