using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Models.TestData;

[ContentTypeAssign(Type = typeof(MockModelMetadataForDecorator))]
internal record MockModelMetadataForDecoratorRaw(int Amount) : RawEntity
{
    protected override IDictionary<string, object?> GetValues() =>
        new Dictionary<string, object?>
        {
            { nameof(Amount), Amount },
            { nameof(MockModelMetadataForDecorator.TargetName), nameof(TargetTypes.Entity) },
            { nameof(MockModelMetadataForDecorator.TargetType), (int)TargetTypes.Entity },
            { nameof(MockModelMetadataForDecorator.DeleteWarning), null! }
        };
}