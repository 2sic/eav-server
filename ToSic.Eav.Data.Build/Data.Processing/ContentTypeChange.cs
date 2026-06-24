namespace ToSic.Eav.Data.Processing;

/// <summary>
/// Describes a saved content-type schema change for low-code actions.
/// </summary>
[PrivateApi("WIP v21")]
public record ContentTypeChange(
    int AppId,
    int ContentTypeId,
    string Source);

public static class ContentTypeChangeSources
{
    public const string ContentType = "content-type";
    public const string ContentTypeField = "content-type-field";
}
