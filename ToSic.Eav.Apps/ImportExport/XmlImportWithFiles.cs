using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Repository.Efc;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.ImportExport
{
    public abstract partial class XmlImportWithFiles: ServiceBase<XmlImportWithFiles.MyServices>
	{
        public class MyServices: MyServicesBase
        {
            public LazySvc<ContentTypeAttributeBuilder> CtAttribBuilder { get; }
            internal readonly LazySvc<Import> ImporterLazy;
            internal readonly LazySvc<DbDataController> DbDataForNewApp;
            internal readonly LazySvc<DbDataController> DbDataForAppImport;
            internal readonly IImportExportEnvironment Environment;
            internal readonly ITargetTypes MetaTargetTypes;
            internal readonly IAppStates AppStates;
            internal readonly LazySvc<XmlToEntity> XmlToEntity;
            internal readonly SystemManager SystemManager;

            public MyServices(
                LazySvc<Import> importerLazy,
                LazySvc<DbDataController> dbDataForNewApp,
                LazySvc<DbDataController> dbDataForAppImport,
                IImportExportEnvironment importExportEnvironment,
                ITargetTypes metaTargetTypes,
                SystemManager systemManager,
                IAppStates appStates,
                LazySvc<XmlToEntity> xmlToEntity,
                LazySvc<ContentTypeAttributeBuilder> ctAttribBuilder
                    )
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
                    SystemManager = systemManager
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
            Messages = new List<Message>();
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
}