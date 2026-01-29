using ToSic.Eav.Models;

namespace ToSic.Eav.Metadata.Recommendations.Sys;

[ModelSpecs(ContentType = ContentTypeNameId)]
internal record MetadataExpectedDecorator : ForExpectedBase
{
    // Informs what Metadata is expected / used on a specific item
    public const string ContentTypeNameId = "c490b369-9cd2-4298-af74-19c1e438cdfc";
    public const string ContentTypeName = "MetadataExpectedDecorator";

    public string Types => Get("MetadataTypes", "");

}