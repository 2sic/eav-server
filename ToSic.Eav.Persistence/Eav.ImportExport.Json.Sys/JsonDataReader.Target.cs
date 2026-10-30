using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonSerializer
{
    private Target DeserializeEntityTarget(JsonEntity jEnt)
    {
        var l = LogDsDetails.Fn<Target>(timer: true);
        if (jEnt.For == null)
            return l.Return(new(), "no for found");

        var mdFor = jEnt.For;

        var targetType = mdFor.TargetType != 0
            ? mdFor.TargetType
            : services.MetadataTargets.GetId(mdFor.Target ?? ""); // #TargetTypeIdInsteadOfTarget, should be deprecated soon

        var target = new Target(
            targetType: targetType,
            title: null,
            keyString: mdFor.String,
            keyNumber: mdFor.Number,
            keyGuid: mdFor.Guid
        );

        return l.Return(target, $"this is metadata; will construct 'For' object. Type: {mdFor.Target} ({mdFor.TargetType})");
    }

}
