using ToSic.Eav.Data.ExtensionsTests.TestData;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.ExtensionsTests;

public partial class EntityListGetAll(TestDataGenerator generator)
{


    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetAllRequiringFactoryMissingFails(int amountMdFor) =>
        Throws<InvalidCastException>(() =>
        {
            var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
            entity.Metadata.GetAll<TestModelRequiringFactoryEmptyConstructor>(
                nameof(TestModelMetadataForDecorator));
        });

}