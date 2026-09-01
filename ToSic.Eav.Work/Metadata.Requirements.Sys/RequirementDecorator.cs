using ToSic.Eav.Models;

namespace ToSic.Eav.Metadata.Requirements.Sys;

[ModelSpecs(ContentType = ContentTypeNameId)]
internal record RequirementDecorator : ModelFromEntity
{
    // Marks Requirements Metadata 13.00
    public const string ContentTypeNameId = "19655377-6626-4986-aea0-ec3c187186ad";

    public string RequirementType => GetThis("");

    public string Feature => GetThis("");

    public string License => GetThis("");

    public string Platform => GetThis("");

    public string SystemCapability => GetThis("");
}