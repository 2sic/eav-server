using ToSic.Eav.Data.ExtensionsTests.TestData;
using ToSic.Eav.Metadata;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.ExtensionsTests;

public class GetOne(TestDataGenerator generator, IWrapperFactory factory)
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void WithSameMetadataManyTimes(int amount)
    {
        var entity = generator.EntityWithMetadataForDecorator(amount);
        var md = entity.Metadata.GetOne<TestModelMetadataForDecorator>();
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void WithSameMetadataManyTimesNamed(int amount)
    {
        var entity = generator.EntityWithMetadataForDecorator(amount);
        var md = entity.Metadata.GetOne<TestModelMetadataForDecorator>(typeName: nameof(TestModelMetadataForDecorator));
        NotNull(md);
        Equal((int)TargetTypes.Entity, md.TargetType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void WithSameMetadataManyTimesNamedWrong(int amount)
    {
        var entity = generator.EntityWithMetadataForDecorator(amount);
        var md = entity.Metadata.GetOne<TestModelMetadataForDecorator>(typeName: "some other name");
        Null(md);
    }


    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetOneRequiringFactoryMissingFails(int amountMdFor) =>
        Throws<InvalidCastException>(() =>
        {
            var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
            entity.Metadata.GetAll<TestModelRequiringFactoryEmptyConstructor>(
                nameof(TestModelMetadataForDecorator));
        });

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetOneRequiringFactory(int amountMdFor)
    {
        var entity = generator.EntityWithMetadataForDecorator(amountMdFor);
        entity.Metadata.GetOne<TestModelRequiringFactoryEmptyConstructor>(
            typeName: nameof(TestModelMetadataForDecorator),
            factory: factory
        );
    }

}