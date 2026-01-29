using ToSic.Eav.Models;

namespace ToSic.Eav.Data.Sys.InputTypes;

/// <summary>
/// Constants related to Input Types
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(state: ShowApiMode.Never)]
[ModelSpecs(ContentType = ContentTypeNameId)]
public record InputTypeDefinition : ModelOfEntity
{
    /// <summary>
    /// Name of the content-type which describes Input-Types
    /// </summary>
    public const string ContentTypeNameId = "ContentType-InputType";

    /// <summary>
    /// Optional CSV of custom configuration types instead of the default cascade
    /// </summary>
    public string? ConfigTypes => GetThis<string>(fallback: null);

    public string? Label => GetThis<string>(fallback: null);

    public string? Description => GetThis<string>(fallback: null);

    public string? Assets => GetThis<string>(fallback: null);

    public bool UseAdam => GetThis(fallback: false);

    public string? AngularAssets => GetThis<string>(fallback: null);

    // ReSharper disable once InconsistentNaming
    public bool DisableI18n => GetThis(fallback: false);

    public string Type => GetThis(fallback: "error-unknown");
}