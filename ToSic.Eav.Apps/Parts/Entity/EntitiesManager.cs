using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Caching;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Lib.DI;

namespace ToSic.Eav.Apps.Parts
{
    /// <inheritdoc />
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public partial class EntitiesManager: PartOf<AppManager>
    {
        #region Constructor / DI

        public EntitiesManager(
            AppWork appWork,
            LazySvc<Import> importLazy,
            SystemManager systemManager,
            LazySvc<IAppLoaderTools> appLoaderTools,
            AppsCacheSwitch appsCache, // Note: Singleton
            LazySvc<JsonSerializer> jsonSerializer
            ) : base("App.EntMan")
        {
            ConnectServices(
                _appWork = appWork,
                _importLazy = importLazy,
                SystemManager = systemManager,
                _appLoaderTools = appLoaderTools,
                _appsCache = appsCache, 
                Serializer = jsonSerializer.SetInit(j => j.SetApp(Parent.AppState))
            );
        }
        private readonly AppWork _appWork;
        private readonly LazySvc<Import> _importLazy;
        private readonly LazySvc<IAppLoaderTools> _appLoaderTools;
        private readonly AppsCacheSwitch _appsCache;
        protected readonly SystemManager SystemManager;
        private LazySvc<JsonSerializer> Serializer { get; }

        private Import DbImporter => _import ?? (_import = _importLazy.Value.Init(Parent.ZoneId, Parent.AppId, false, false));
        private Import _import;

        #endregion
    }
}
