using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models;

public class EntityAsTests(TestDataGenerator generator)
{

    [Fact]
    public void AsBasic()
    {
        var entity = generator.CreateMetadataForDecorator();
        var model = entity.AsTac<TestModelMetadataForDecorator>();
        NotNull(model);
        Equal((int)TargetTypes.Entity, model.TargetType);
    }

    [Fact]
    public void AsNullCapable() =>
        NotNull(((IEntity)null!).AsTac<TestModelNullCapable>());

    [Fact]
    public void AsNullUnCapable() =>
        Null(((IEntity)null!).AsTac<TestModelNullUnCapable>());


    [Fact]
    public void AsWrongTypeThrows() =>
        Throws<InvalidCastException>(() =>
            generator.CreateMetadataForDecorator()
                .AsTac<TestModelMetadataForDecoratorWrongName>()
        );

    [Fact]
    public void AsNullPreferNull() =>
        Null(((IEntity)null!).AsTac<TestModelNullCapable>(nullIfNull: true));

    [Fact]
    public void AsWrongTypeSkipCheckWorks()
    {
        var model = generator.CreateMetadataForDecorator()
            .AsTac<TestModelMetadataForDecoratorWrongName>(skipTypeCheck: true);
        NotNull(model);
        Equal((int)TargetTypes.Entity, model.TargetType);
    }

    [Fact]
    public void AsRequiresFactoryThrows() =>
        Throws<InvalidCastException>(() =>
            generator.CreateMetadataForDecorator()
                .AsTac<TestModelRequiringFactoryEmptyConstructor>()
        );


}