using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Data.Build;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Repositories.Sys;

namespace ToSic.Eav.ImportExport.Sys.XmlImport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class XmlImportWithFiles(XmlImportWithFiles.Dependencies services)
    : ServiceBase<XmlImportWithFiles.Dependencies>(services, "Xml.ImpFil")
{
    public class Dependencies(
        LazySvc<ImportService> importerLazy,
        IStorageFactory storageFactory,
        IImportExportEnvironment importExportEnvironment,
        AppCachePurger appCachePurger,
        IAppsCatalog appsCatalog,
        LazySvc<XmlToEntity> xmlToEntity,
        LazySvc<DataBuilder> multiBuilder)
        : DependenciesBase(connect: [importerLazy, storageFactory, importExportEnvironment, appsCatalog, xmlToEntity, appCachePurger, multiBuilder])
    {
        public IStorageFactory StorageFactory { get; } = storageFactory;
        public readonly LazySvc<DataBuilder> MultiBuilder = multiBuilder;
        internal readonly LazySvc<ImportService> ImporterLazy = importerLazy;
        internal readonly IImportExportEnvironment Environment = importExportEnvironment;
        internal readonly IAppsCatalog AppsCatalog = appsCatalog;
        internal readonly LazySvc<XmlToEntity> XmlToEntity = xmlToEntity;
        internal readonly AppCachePurger AppCachePurger = appCachePurger;
    }

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