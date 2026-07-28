using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entity;

public class ToModelTests(TestDataGenerator generator)
{

    [Fact]
    public void AsBasic()
    {
        var entity = generator.CreateMetadataForDecorator();
        var model = entity.ToModelTac<MockModelMetadataForDecorator>();
        NotNull(model);
        Equal((int)TargetTypes.Entity, model.TargetType);
    }


    [Fact]
    public void AsWrongTypeThrows() =>
        Throws<InvalidCastException>(() =>
            generator.CreateMetadataForDecorator()
                .ToModelTac<MockModelMetadataForDecoratorWrongName>()
        );

    [Fact]
    public void AsWrongTypeSkipCheckWorks()
    {
        var model = generator.CreateMetadataForDecorator()
            .ToModelTac<MockModelMetadataForDecoratorWrongName>(skipTypeCheck: true);
        NotNull(model);
        Equal((int)TargetTypes.Entity, model.TargetType);
    }


}