using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Data.Build;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.ImportExport.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract partial class XmlImportWithFiles: ServiceBase<XmlImportWithFiles.MyServices>
{
    public class MyServices: MyServicesBase
    {
        public readonly LazySvc<DataBuilder> MultiBuilder;
        public LazySvc<ContentTypeAttributeBuilder> CtAttribBuilder { get; }
        internal readonly LazySvc<ImportService> ImporterLazy;
        internal readonly LazySvc<DbDataController> DbDataForNewApp;
        internal readonly LazySvc<DbDataController> DbDataForAppImport;
        internal readonly IImportExportEnvironment Environment;
        internal readonly ITargetTypes MetaTargetTypes;
        internal readonly IAppStates AppStates;
        internal readonly LazySvc<XmlToEntity> XmlToEntity;
        internal readonly AppCachePurger AppCachePurger;

        public MyServices(
            LazySvc<ImportService> importerLazy,
            LazySvc<DbDataController> dbDataForNewApp,
            LazySvc<DbDataController> dbDataForAppImport,
            IImportExportEnvironment importExportEnvironment,
            ITargetTypes metaTargetTypes,
            AppCachePurger appCachePurger,
            IAppStates appStates,
            LazySvc<XmlToEntity> xmlToEntity,
            LazySvc<ContentTypeAttributeBuilder> ctAttribBuilder,
            LazySvc<DataBuilder> multiBuilder)
        {
            ConnectServices(
                CtAttribBuilder = ctAttribBuilder,
                ImporterLazy = importerLazy,
                DbDataForNewApp = dbDataForNewApp,
                DbDataForAppImport = dbDataForAppImport,
                Environment = importExportEnvironment,
                MetaTargetTypes = metaTargetTypes,
                AppStates = appStates,
                XmlToEntity = xmlToEntity,
                AppCachePurger = appCachePurger,
                MultiBuilder = multiBuilder
            );
        }
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
        Messages = new();
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