using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.DataSource;
using ToSic.Eav.Repository.Efc;
using ToSic.Lib.DI;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Work
{
    public class AppWork : ServiceBase
    {
        public AppWorkContextService CtxSvc { get; }
        private readonly Generator<WorkEntityPublish> _genEntityPublish;
        private readonly Generator<WorkEntityDelete> _genEntityDelete;
        private readonly Generator<WorkFieldList> _genEntityFieldList;
        private readonly Generator<WorkMetadata> _genEntityMetadata;
        private readonly Generator<WorkEntityUpdate> _genEntityUpdate;
        private readonly Generator<WorkEntityCreate> _genEntityCreate;
        private readonly Generator<WorkEntitySave> _genEntitySave;
        private readonly Generator<AppInputTypes> _genInputType;
        private readonly Generator<ExportListXml> _exportListXmlGenerator;
        private readonly Generator<AppEntityRead> _genEntity;

        public AppWork(
            AppWorkContextService ctxSvc,
            Generator<AppEntityRead> genEntity,
            Generator<AppInputTypes> genInputType,
            Generator<ExportListXml> exportListXmlGenerator,
            Generator<WorkEntitySave> genEntitySave,
            Generator<WorkEntityCreate> genEntityCreate,
            Generator<WorkEntityUpdate> genEntityUpdate,
            Generator<WorkMetadata> genEntityMetadata,
            Generator<WorkFieldList> genEntityFieldList,
            Generator<WorkEntityDelete> genEntityDelete,
            Generator<WorkEntityPublish> genEntityPublish) : base("App.SysCtF")
        {
            ConnectServices(
                CtxSvc = ctxSvc,
                _genEntity = genEntity,
                _genInputType = genInputType,
                _exportListXmlGenerator = exportListXmlGenerator,
                _genEntitySave = genEntitySave,
                _genEntityCreate = genEntityCreate,
                _genEntityUpdate = genEntityUpdate,
                _genEntityMetadata = genEntityMetadata,
                _genEntityFieldList = genEntityFieldList,
                _genEntityDelete = genEntityDelete,
                _genEntityPublish = genEntityPublish
            );
        }

        #region Context


        public IAppWorkCtx Context(int appId) => CtxSvc.Context(appId);
        public IAppWorkCtxPlus ContextPlus(int appId, bool? showDrafts = default, IDataSource data = default)
            => CtxSvc.ContextPlus(appId, showDrafts, data);

        public IAppWorkCtx Context(IAppIdentity appIdentity) => CtxSvc.Context(appIdentity);

        public IAppWorkCtxPlus ContextPlus(IAppIdentity appIdentity, bool? showDrafts = default, IDataSource data = default)
            => CtxSvc.ContextPlus(appIdentity, showDrafts, data);

        public IAppWorkCtxPlus ToCtxPlus(IAppWorkCtx appCtx, bool? showDrafts = default, IDataSource data = default)
            => CtxSvc.ToCtxPlus(appCtx, showDrafts, data);

        public IAppWorkCtxWithDb CtxWithDb(AppState appState, DbDataController existingDb = default)
            => CtxSvc.CtxWithDb(appState, existingDb);

        #endregion

        #region Sys Parts Factories

        public AppEntityRead Entities => _entRead.Get(() => _genEntity.New());
        private readonly GetOnce<AppEntityRead> _entRead = new GetOnce<AppEntityRead>();

        public AppInputTypes InputTypes => _inputTypes.Get(() => _genInputType.New());
        private readonly GetOnce<AppInputTypes> _inputTypes = new GetOnce<AppInputTypes>();

        public ExportListXml EntityXmlExporter(IAppWorkCtx appCtx, string contentType)
            => _exportListXmlGenerator.New().Init(appCtx.AppState, appCtx.AppState.GetContentType(contentType));

        public AppEntityRead EntityRead() => _genEntity.New();


        public WorkEntityCreate EntityCreate(IAppWorkCtxWithDb appCtx) => _genEntityCreate.New().InitContext(appCtx);

        public WorkEntitySave EntitySave(IAppWorkCtxWithDb appCtx) => _genEntitySave.New().InitContext(appCtx);

        public WorkEntitySave EntitySave(AppState appState) => _genEntitySave.New().InitContext(CtxWithDb(appState));

        public WorkEntitySave EntitySave(AppState appState, DbDataController existingController)
            => _genEntitySave.New().InitContext(CtxWithDb(appState, existingController));

        public WorkEntityUpdate EntityUpdate(IAppWorkCtxWithDb ctx, AppState appState = default)
            => _genEntityUpdate.New().InitContext(ctx ?? CtxWithDb(appState));

        public WorkMetadata EntityMetadata(IAppWorkCtxWithDb ctx, AppState appState = default)
            => _genEntityMetadata.New().InitContext(ctx ?? CtxWithDb(appState));

        public WorkFieldList EntityFieldList(IAppWorkCtxWithDb ctx = default, AppState appState = default)
            => _genEntityFieldList.New().InitContext(ctx ?? CtxWithDb(appState));

        public WorkEntityDelete EntityDelete(IAppWorkCtxWithDb ctx = default, AppState appState = default)
            => _genEntityDelete.New().InitContext(ctx ?? CtxWithDb(appState));

        public WorkEntityPublish EntityPublish(IAppWorkCtxWithDb ctx = default, AppState appState = default)
            => _genEntityPublish.New().InitContext(ctx ?? CtxWithDb(appState));

        #endregion
    }
}
