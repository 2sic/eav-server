namespace ToSic.Eav.Data.Processing;

/// <summary>
/// Describes a saved content-type schema change for low-code actions.
/// </summary>
[PrivateApi("WIP v21")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record ContentTypeChange(
    int AppId,
    string ContentTypeNameId,
    string Source);

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ContentTypeChangeSources
{
    public const string ContentType = "content-type";
    public const string ContentTypeField = "content-type-field";
}
