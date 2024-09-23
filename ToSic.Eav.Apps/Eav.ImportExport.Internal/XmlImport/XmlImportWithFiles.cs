using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Data.Build;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.ImportExport.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract partial class XmlImportWithFiles: ServiceBase<XmlImportWithFiles.MyServices>
{
    public class MyServices(
        LazySvc<ImportService> importerLazy,
        LazySvc<DbDataController> dbDataForNewApp,
        LazySvc<DbDataController> dbDataForAppImport,
        IImportExportEnvironment importExportEnvironment,
        ITargetTypes metaTargetTypes,
        AppCachePurger appCachePurger,
        IAppsCatalog appsCatalog,
        LazySvc<XmlToEntity> xmlToEntity,
        LazySvc<DataBuilder> multiBuilder)
        : MyServicesBase(connect:
        [
            importerLazy, dbDataForNewApp, dbDataForAppImport, importExportEnvironment,
            metaTargetTypes, appsCatalog, xmlToEntity, appCachePurger, multiBuilder
        ])
    {
        public readonly LazySvc<DataBuilder> MultiBuilder = multiBuilder;
        internal readonly LazySvc<ImportService> ImporterLazy = importerLazy;
        internal readonly LazySvc<DbDataController> DbDataForNewApp = dbDataForNewApp;
        internal readonly LazySvc<DbDataController> DbDataForAppImport = dbDataForAppImport;
        internal readonly IImportExportEnvironment Environment = importExportEnvironment;
        internal readonly ITargetTypes MetaTargetTypes = metaTargetTypes;
        internal readonly IAppsCatalog AppsCatalog = appsCatalog;
        internal readonly LazySvc<XmlToEntity> XmlToEntity = xmlToEntity;
        internal readonly AppCachePurger AppCachePurger = appCachePurger;
    }

    private List<DimensionDefinition> _targetDimensions;
    private DbDataController _eavContext;
    public int AppId { get; private set; }
    public int ZoneId { get; private set; }


    private XmlToEntity _xmlBuilder;

    /// <summary>
    /// The default language / culture - example: de-DE
    /// </summary>
    private string DefaultLanguage { get; set; }

    private bool AllowUpdateOnSharedTypes { get; set; }

    #region Constructor / DI
    /// <summary>
    /// constructor, not DI
    /// </summary>
    protected XmlImportWithFiles(MyServices services, string logName = null) : base(services, logName ?? "Xml.ImpFil")
    {
    }

    /// <summary>
    /// Create a new xmlImport instance
    /// </summary>
    /// <param name="parentLog"></param>
    /// <param name="defaultLanguage">The portals default language / culture - example: de-DE</param>
    /// <param name="allowUpdateOnSharedTypes">Specify if the import should be able to change system-wide things like shared attributesets</param>
    public XmlImportWithFiles Init(string defaultLanguage, bool allowUpdateOnSharedTypes)
    {
        // Prepare
        Messages = [];
        DefaultLanguage = (defaultLanguage ?? base.Services.Environment.DefaultLanguage).ToLowerInvariant();
        AllowUpdateOnSharedTypes = allowUpdateOnSharedTypes;
        return this;
    }
    #endregion


    #region Helpers to keep architecture clean for now  - special for template-save in 2sxc

    protected bool RepositoryHasEntity(Guid entityGuid)
        => _eavContext.Entities.EntityExists(entityGuid);

    protected int GetLatestRepositoryId(Guid entityGuid)
        => _eavContext.Entities.GetMostCurrentDbEntity(entityGuid).EntityId;

    #endregion
}