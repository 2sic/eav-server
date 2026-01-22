namespace ToSic.Eav.Metadata.Recommendations.Sys;

/// <summary>
/// WIP v21: Decorator to provide Metadata access to decorated IEntity
/// </summary>
[PrivateApi("still WIP / internal")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal record MetadataForDecorator: ForExpectedBase
{
    public static string ContentTypeNameId = "4c88d78f-5f3e-4b66-95f2-6d63b7858847";
    public static string ContentTypeName = "MetadataForDecorator";

    public string TargetName => GetThis("");
}
