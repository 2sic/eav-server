using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.CodeContentTypes;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Metadata;

public static class MetadataSamples
{
    public static IEntity CreateMetadataForDecorator(DataBuilder builder, ContentTypeFactory ctFactory)
    {
        var ct = ctFactory.CreateTac<MetadataForDecoratorMock>();

        return builder.CreateEntityTac(0, ct, values: new()
        {
            { nameof(MetadataForDecoratorMock.Amount), 1 },
            { nameof(MetadataForDecoratorMock.TargetName), nameof(TargetTypes.Entity) },
            { nameof(MetadataForDecoratorMock.TargetType), (int)TargetTypes.Entity },
            { nameof(MetadataForDecoratorMock.DeleteWarning), null! }
        });
    }

    public static IEntity CreateEntityForNoSpecs(DataBuilder builder, ContentTypeFactory ctFactory)
    {
        var ct = ctFactory.CreateTac<CodeTypeNoSpecs>();

        return builder.CreateEntityTac(0, ct, values: new()
        {
            { nameof(CodeTypeNoSpecs.Id), 1 },
            { nameof(CodeTypeNoSpecs.Name), "Test" },
            { nameof(CodeTypeNoSpecs.Age), 30 },
            { nameof(CodeTypeNoSpecs.BirthDate), new DateTime(1990, 1, 1) },
            { nameof(CodeTypeNoSpecs.IsAlive), true }
        });
    }
}