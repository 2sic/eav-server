using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Dimensions.Sys;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Repositories.Sys;

namespace ToSic.Eav.ImportExport.Sys.XmlImport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class XmlImportWithFiles(XmlImportWithFiles.MyServices services)
    : ServiceBase<XmlImportWithFiles.MyServices>(services, "Xml.ImpFil")
{
    public class MyServices(
        LazySvc<ImportService> importerLazy,
        IStorageFactory storageFactory,
        IImportExportEnvironment importExportEnvironment,
        //ITargetTypeService metaTargetTypes,
        AppCachePurger appCachePurger,
        IAppsCatalog appsCatalog,
        LazySvc<XmlToEntity> xmlToEntity,
        LazySvc<DataBuilder> multiBuilder)
        : MyServicesBase(connect:
        [
            importerLazy, storageFactory, importExportEnvironment,
            /*metaTargetTypes,*/ appsCatalog, xmlToEntity, appCachePurger, multiBuilder
        ])
    {
        public IStorageFactory StorageFactory { get; } = storageFactory;
        public readonly LazySvc<DataBuilder> MultiBuilder = multiBuilder;
        internal readonly LazySvc<ImportService> ImporterLazy = importerLazy;
        internal readonly IImportExportEnvironment Environment = importExportEnvironment;
        //internal readonly ITargetTypeService MetaTargetTypes = metaTargetTypes;
        internal readonly IAppsCatalog AppsCatalog = appsCatalog;
        internal readonly LazySvc<XmlToEntity> XmlToEntity = xmlToEntity;
        internal readonly AppCachePurger AppCachePurger = appCachePurger;
    }

    private List<DimensionDefinition> _targetDimensions;
    public int AppId { get; private set; }
    public int ZoneId { get; private set; }


    private XmlToEntity XmlBuilder
    {
        get => field ?? throw new("XmlBuilder not initialized, please call Init() first.");
        set;
    }

    /// <summary>
    /// The default language / culture - example: de-DE
    /// </summary>
    private string DefaultLanguage { get; set; }

    private bool AllowUpdateOnSharedTypes { get; set; }

    #region Constructor / DI

    /// <summary>
    /// Create a new xmlImport instance
    /// </summary>
    /// <param name="defaultLanguage">The portals default language / culture - example: de-DE</param>
    /// <param name="allowUpdateOnSharedTypes">Specify if the import should be able to change system-wide things like shared content-types</param>
    public XmlImportWithFiles Init(string defaultLanguage, bool allowUpdateOnSharedTypes)
    {
        // Prepare
        Messages = [];
        DefaultLanguage = (defaultLanguage ?? base.Services.Environment.DefaultLanguage).ToLowerInvariant();
        AllowUpdateOnSharedTypes = allowUpdateOnSharedTypes;
        return this;
    }
    #endregion

}