using ToSic.Eav.Models;

namespace ToSic.Eav.Metadata.Recommendations.Sys;

/// <summary>
/// WIP v21: Decorator to provide Metadata access to decorated IEntity
/// </summary>
[PrivateApi("still WIP / internal")]
[ShowApiWhenReleased(ShowApiMode.Never)]
[ModelSpecs(ContentType = ContentTypeNameId)]
internal record MetadataForDecorator: ForExpectedBase
{
    public const string ContentTypeNameId = "4c88d78f-5f3e-4b66-95f2-6d63b7858847";
    public const string ContentTypeName = "MetadataForDecorator";

    public string TargetName => GetThis("");
}
