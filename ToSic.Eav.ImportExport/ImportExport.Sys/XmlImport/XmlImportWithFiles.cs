using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.LogSettings;
using ToSic.Eav.Data.Build;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Repositories.Sys;

namespace ToSic.Eav.ImportExport.Sys.XmlImport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class XmlImportWithFiles(XmlImportWithFiles.Dependencies services)
    : ServiceBase<XmlImportWithFiles.Dependencies>(services, "Xml.ImpFil")
{
    public record Dependencies(
        LazySvc<ImportService> ImporterLazy,
        Generator<IStorage, StorageOptions> StorageFactory,
        IImportExportEnvironment Environment,
        AppCachePurger AppCachePurger,
        IAppsCatalog AppsCatalog,
        LazySvc<XmlToEntity> XmlToEntity,
        LazySvc<DataBuilder> MultiBuilder,
        DataImportLogSettings LogSettings)
        : DependenciesRecord(connect: [ImporterLazy, StorageFactory, Environment, AppsCatalog, XmlToEntity, AppCachePurger, MultiBuilder, LogSettings]);

    public int AppId { get; private set; }
    public int ZoneId { get; private set; }

    [field: AllowNull, MaybeNull]
    private XmlToEntity XmlBuilder
    {
        get => field ?? throw new("XmlBuilder not initialized, please call Init() first.");
        set;
    }

    /// <summary>
    /// The default language / culture - example: de-DE
    /// </summary>
    private string DefaultLanguage { get; set; } = null!;

    private bool AllowUpdateOnSharedTypes { get; set; }

    #region Detailed Logging

    [field: AllowNull, MaybeNull]
    private LogSettings LogSettings => field ??= Services.LogSettings.GetLogSettings();

    /// <summary>
    /// Logger for the details of the deserialization process.
    /// Goal is that it can be enabled/disabled as needed.
    /// </summary>
    internal ILog? LogDetails => field ??= Log.IfDetails(LogSettings);

    internal ILog? LogSummary => field ??= Log.IfSummary(LogSettings);

    #endregion

    #region Constructor / DI

    /// <summary>
    /// Create a new xmlImport instance
    /// </summary>
    /// <param name="defaultLanguage">The portals default language / culture - example: de-DE</param>
    /// <param name="allowUpdateOnSharedTypes">Specify if the import should be able to change system-wide things like shared content-types</param>
    public XmlImportWithFiles Init(string? defaultLanguage, bool allowUpdateOnSharedTypes)
    {
        // Prepare
        DefaultLanguage = (defaultLanguage ?? Services.Environment.DefaultLanguage).ToLowerInvariant();
        AllowUpdateOnSharedTypes = allowUpdateOnSharedTypes;
        return this;
    }
    #endregion

}