﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.ImportExport
{
    public abstract partial class XmlImportWithFiles: HasLog
	{
        public class Dependencies
        {

            public Dependencies(
                Lazy<Import> importerLazy,
                Lazy<DbDataController> dbDataForNewApp,
                Lazy<DbDataController> dbDataForAppImport,
                IImportExportEnvironment importExportEnvironment,
                ITargetTypes metaTargetTypes,
                SystemManager systemManager,
                IAppStates appStates,
                IAppsCache appsCache
                )
            {
                AppsCache = appsCache;
                _importerLazy = importerLazy;
                _dbDataForNewApp = dbDataForNewApp;
                _dbDataForAppImport = dbDataForAppImport;
                _environment = importExportEnvironment;
                _metaTargetTypes = metaTargetTypes;
                AppStates = appStates;
                SystemManager = systemManager;//.Init(Log);
                //_environment.LinkLog(Log);
            }
            internal readonly IAppsCache AppsCache;
            internal readonly Lazy<Import> _importerLazy;
            internal readonly Lazy<DbDataController> _dbDataForNewApp;
            internal readonly Lazy<DbDataController> _dbDataForAppImport;
            internal readonly IImportExportEnvironment _environment;
            internal readonly ITargetTypes _metaTargetTypes;
            internal readonly IAppStates AppStates;
            internal readonly SystemManager SystemManager;

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
        protected XmlImportWithFiles(
            Dependencies dependencies,
            //Lazy<Import> importerLazy, 
            //Lazy<DbDataController> dbDataForNewApp,
            //Lazy<DbDataController> dbDataForAppImport,
            //IImportExportEnvironment importExportEnvironment,
            //ITargetTypes metaTargetTypes,
            //SystemManager systemManager,
            //IAppStates appStates,
            string logName = null) : base(logName ?? "Xml.ImpFil")
        {
             // todo: finish switch to dependencies

            //_importerLazy = importerLazy;
            //_dbDataForNewApp = dbDataForNewApp;
            //_dbDataForAppImport = dbDataForAppImport;
            //_environment = importExportEnvironment;
            //_metaTargetTypes = metaTargetTypes;
            //AppStates = appStates;


            //SystemManager = systemManager.Init(Log);
            //_environment.LinkLog(Log);
            Deps = dependencies;
            dependencies.SystemManager.Init(Log);
            dependencies._environment.LinkLog(Log);
        }

        protected Dependencies Deps;
        //private readonly Lazy<Import> _importerLazy;
        //private readonly Lazy<DbDataController> _dbDataForNewApp;
        //private readonly Lazy<DbDataController> _dbDataForAppImport;
        //private readonly IImportExportEnvironment _environment;
        //private readonly ITargetTypes _metaTargetTypes;
        //protected readonly IAppStates AppStates;
        //protected readonly SystemManager SystemManager;


        /// <summary>
	    /// Create a new xmlImport instance
	    /// </summary>
	    /// <param name="parentLog"></param>
	    /// <param name="defaultLanguage">The portals default language / culture - example: de-DE</param>
	    /// <param name="allowUpdateOnSharedTypes">Specify if the import should be able to change system-wide things like shared attributesets</param>
        public XmlImportWithFiles Init(string defaultLanguage, bool allowUpdateOnSharedTypes, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            // Prepare
            Messages = new List<Message>();
            DefaultLanguage = (defaultLanguage ?? Deps._environment.DefaultLanguage).ToLowerInvariant();
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