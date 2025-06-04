using ToSic.Eav.Data.EntityBased.Sys;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.ImportExport.Sys;

/// <summary>
/// Metadata decorator for entities / content-types to mark them for export in a bundle
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExportConfiguration(IEntity entity) : EntityBasedType(entity)
{
    public const string ContentTypeId = "d7f2e4fa-5306-41bb-a3cd-d9529c838879";
    public const string ContentTypeName = "🧑‍💻SystemExportConfiguration";

    /// <summary>
    /// Name of the configuration - just fyi
    /// </summary>
    public string Name => GetThis("");

    /// <summary>
    /// Determine if we should keep or remove the Export-Decorators when exporting.
    /// Only relevant if `WithMetadata` is true (default)
    /// </summary>
    public bool PreserveMarkers => GetThis(false);
        
    /// <summary>
    /// Expected file name.
    /// </summary>
    public string FileName => GetThis("bundle.json");

    /// <summary>
    /// Export with Metadata
    /// </summary>
    public bool EntitiesWithMetadata => GetThis(true);

    /// <summary>
    /// Find all decorator metadata of type SystemExportDecorator
    /// </summary>
    public List<ExportDecorator> ExportMarkers => _exportMarkers.Get(() => Entity.Parents(ExportDecorator.TypeNameId).Select(e => new ExportDecorator(e)).ToList());
    private readonly GetOnce<List<ExportDecorator>> _exportMarkers = new();

    /// <summary>
    /// Content Types to Export
    /// </summary>
    public List<string> ContentTypes => ExportMarkers
        .Where(e => e.IsContentType && !string.IsNullOrEmpty(e.KeyString))
        .Select(et => et.KeyString).ToList();
        
    /// <summary>
    /// Entities to Export
    /// </summary>
    public List<Guid> Entities => ExportMarkers
        .Where(e => e.IsEntity && e.KeyGuid.HasValue)
        .Select(et => et.KeyGuid.Value).ToList();
}