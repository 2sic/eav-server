using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Models.TestData;

[ContentTypeUse(Type = typeof(MockMetadataModel))]
internal record MockMetadataRaw(int Amount) : RawEntity
{
    protected override IDictionary<string, object?> GetValues() =>
        new Dictionary<string, object?>
        {
            { nameof(Amount), Amount },
            { nameof(MockMetadataModel.TargetName), nameof(TargetTypes.Entity) },
            { nameof(MockMetadataModel.TargetType), (int)TargetTypes.Entity },
            { nameof(MockMetadataModel.DeleteWarning), null! }
        };
}
