using System;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.DataSource;
using ToSic.Eav.Services;
using ToSic.Lib.DI;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.AppSys
{
    public class AppWork: ServiceBase
    {
        private readonly Generator<AppInputTypes> _genInputType;
        private readonly Generator<ExportListXml> _exportListXmlGenerator;
        private readonly Generator<AppEntityRead> _genEntity;
        private readonly Generator<AppContentTypes> _genType;
        private readonly IDataSourcesService _dataSourcesService;
        private readonly LazySvc<IAppStates> _appStates;

        public AppWork(
            IDataSourcesService dataSourcesService,
            LazySvc<IAppStates> appStates,
            Generator<AppEntityRead> genEntity,
            Generator<AppContentTypes> genType,
            Generator<AppInputTypes> genInputType,
            Generator<ExportListXml> exportListXmlGenerator


            ) : base("App.SysCtF")
        {
            ConnectServices(
                _dataSourcesService = dataSourcesService,
                _appStates = appStates,
                _genEntity = genEntity,
                _genType = genType,
                _genInputType = genInputType,
                _exportListXmlGenerator = exportListXmlGenerator
            );
        }

        #region Public Helpers - commonly used to get App System Context

        public IAppStates AppStates => _appStates.Value;

        #endregion

        #region Context

        public IAppWorkCtx Ctx(IAppWorkCtx ctx = default, IAppIdentity identity = default, AppState state = default, int? appId = default)
        {
            if (ctx != null) return ctx;
            if (state != null) return Context(state);
            if (identity != null) return Context(identity);
            if (appId != null) return Context(appId.Value);
            throw new ArgumentException("Some of the identity arguments must be provided.");
        }

        public IAppWorkCtxPlus CtxPlus(IAppWorkCtx ctx = default, IAppIdentity identity = default, AppState state = default, int? appId = default)
        {
            if (ctx is IAppWorkCtxPlus asPlus) return asPlus;
            if (ctx != null) return ContextPlus(ctx.AppState);
            if (state != null) return ContextPlus(state);
            if (identity != null) return ContextPlus(identity);
            if (appId != null) return ContextPlus(appId.Value);
            throw new ArgumentException("Some of the identity arguments must be provided.");
        }

        public IAppWorkCtx Context(AppState appState) => new AppWorkCtx(appState);
        public IAppWorkCtxPlus ContextPlus(AppState appState, bool? showDrafts = default, IDataSource data = default)
            => new AppWorkCtxPlus(_dataSourcesService, appState, showDrafts, data);

        public IAppWorkCtx Context(int appId) => new AppWorkCtx(_appStates.Value.Get(appId));
        public IAppWorkCtxPlus ContextPlus(int appId, bool? showDrafts = default, IDataSource data = default)
            => new AppWorkCtxPlus(_dataSourcesService, appState: _appStates.Value.Get(appId), showDrafts, data);

        public IAppWorkCtx Context(IAppIdentity appIdentity) => new AppWorkCtx(_appStates.Value.KeepOrGet(appIdentity));
        public IAppWorkCtxPlus ContextPlus(IAppIdentity appIdentity, bool? showDrafts = default, IDataSource data = default)
            => new AppWorkCtxPlus(_dataSourcesService, _appStates.Value.KeepOrGet(appIdentity), showDrafts, data);

        public IAppWorkCtxPlus ToCtxPlus(IAppWorkCtx appCtx, bool? showDrafts = default, IDataSource data = default)
            => new AppWorkCtxPlus(appCtx, _dataSourcesService, appCtx.AppState, showDrafts, data);

        #endregion

        #region Sys Parts Factories

        public AppEntityRead Entities => _entRead.Get(() => _genEntity.New());
        private readonly GetOnce<AppEntityRead> _entRead = new GetOnce<AppEntityRead>();

        public AppContentTypes ContentTypes => _typeRead.Get(() => _genType.New());
        private readonly GetOnce<AppContentTypes> _typeRead = new GetOnce<AppContentTypes>();

        public AppInputTypes InputTypes => _inputTypes.Get(() => _genInputType.New());
        private readonly GetOnce<AppInputTypes> _inputTypes = new GetOnce<AppInputTypes>();

        public ExportListXml EntityXmlExporter(IAppWorkCtx appCtx, string contentType)
            => _exportListXmlGenerator.New().Init(appCtx.AppState, appCtx.AppState.GetContentType(contentType));

        public AppEntityRead EntityRead() => _genEntity.New();

        public AppContentTypes ContentTypeRead() => _genType.New();

        #endregion
    }
}
