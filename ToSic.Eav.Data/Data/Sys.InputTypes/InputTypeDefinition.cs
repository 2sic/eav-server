using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Model;

namespace ToSic.Eav.Data.Sys.InputTypes;

/// <summary>
/// Constants related to Input Types
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(state: ShowApiMode.Never)]
public record InputTypeDefinition : ModelOfEntity
{
    /// <summary>
    /// Constants related to Input Types
    /// </summary>
    public InputTypeDefinition(IEntity entity) : base(entity: entity) { }

    /// <summary>
    /// Name of the content-type which describes Input-Types
    /// </summary>
    public const string TypeForInputTypeDefinition = "ContentType-InputType";

    /// <summary>
    /// Optional CSV of custom configuration types instead of the default cascade
    /// </summary>
    public string? ConfigTypes => GetThis(fallback: null as string);

    public string? Label => GetThis(fallback: null as string);

    public string? Description => GetThis(fallback: null as string);

    public string? Assets => GetThis(fallback: null as string);

    public bool UseAdam => GetThis(fallback: false);

    public string? AngularAssets => GetThis(fallback: null as string);

    // ReSharper disable once InconsistentNaming
    public bool DisableI18n => GetThis(fallback: false);

    public string Type => GetThis(fallback: "error-unknown");
}