using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Internal.MetadataDecorators;

internal class RequirementDecorator(IEntity entity) : EntityBasedType(entity)
{
    // Marks Requirements Metadata 13.00
    public static string TypeName = "19655377-6626-4986-aea0-ec3c187186ad";

    public const string ReqFeature = "feature";
    public const string ReqLicense = "license";
    public const string ReqPlatform = "platform";
    public const string ReqSysCap = "systemcapability";
    public const string ReqNone = "none";
    public const string ReqUnknown = "unknown";

    public string RequirementType => GetThis("");

    public string Feature => GetThis("");

    public string License => GetThis("");

    public string Platform => GetThis("");

    public string SystemCapability => GetThis("");
}