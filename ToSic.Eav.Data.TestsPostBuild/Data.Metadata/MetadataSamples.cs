using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Metadata;

public static class MetadataSamples
{
    public static IEntity CreateMetadataForDecorator(DataBuilder builder, ContentTypeFactory ctFactory)
    {
        var ct = ctFactory.CreateTac<MetadataForDecorator>();

        return builder.CreateEntityTac(0, ct, values: new()
        {
            { nameof(MetadataForDecorator.Amount), 1 },
            { nameof(MetadataForDecorator.TargetName), nameof(TargetTypes.Entity) },
            { nameof(MetadataForDecorator.TargetType), (int)TargetTypes.Entity },
            { nameof(MetadataForDecorator.DeleteWarning), null! }
        });
    }
}