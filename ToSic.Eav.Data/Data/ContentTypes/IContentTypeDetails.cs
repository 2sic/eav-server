using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Models;

namespace ToSic.Eav.Data.ContentTypes;

/// <summary>
/// Content Type Settings as typically configured in the UI
/// </summary>
[ModelSpecs(
    ContentType = Constants.ContentTypeName,
    Use = typeof(ContentTypeDetailsModel)
)]
public interface IContentTypeDetails: IModelFromEntity
{
    [PrivateApi]
    public class Constants
    {
        public const string ContentTypeName = "ContentType";
    }

    /// <summary>
    /// The visible label / title of the content type, as it appears in the UI.
    /// </summary>
    string Label { get; }

    /// <summary>
    /// Brief description/teaser (not html) - mainly tables of Content-Types.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Additional notes (HTML) - mainly for the admin.
    /// </summary>
    /// <remarks>
    /// Usually a file (png) referenced like `[App:Path]/ct-dnn-icon.png` when in the app.
    /// In complex scenarios, there are also undocumented ways to include an SVG directly inline...
    /// </remarks>
    string? Notes { get; }

    /// <summary>
    /// The icon representing the content type, for showing in selection-UIs.
    /// </summary>
    string? Icon { get; }

    /// <summary>
    /// The link for additional tutorials, instructions etc. (probably not used anywhere, but not sure)
    /// </summary>
    string? Link { get; }

    /// <summary>
    /// Instructions to show in the edit-dialog of items of this content type.
    /// </summary>
    string? EditInstructions { get; }

    /// <summary>
    /// Instructions to show when listing items of this content type - I believe currently not implemented.
    /// </summary>
    string? ListInstructions { get; }

    /// <summary>
    /// Lists all names of settings (AppSettings) to load when editing this content-type, as it will be needed in Formulas.
    /// </summary>
    string? AdditionalSettings { get; }

    /// <summary>
    /// Internal / secret / complicated feature.
    /// </summary>
    [PrivateApi]
    string? DynamicChildrenField { get; }
}