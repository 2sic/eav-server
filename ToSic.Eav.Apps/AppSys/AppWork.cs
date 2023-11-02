using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.DataSource;
using ToSic.Eav.Repository.Efc;
using ToSic.Lib.DI;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.AppSys
{
    public class AppWork: ServiceBase
    {
        public AppWorkContextService CtxSvc { get; }
        private readonly Generator<EntityWorkPublish> _genEntityPublish;
        private readonly Generator<EntityWorkDelete> _genEntityDelete;
        private readonly Generator<EntityWorkFieldList> _genEntityFieldList;
        private readonly Generator<EntityWorkMetadata> _genEntityMetadata;
        private readonly Generator<EntityWorkUpdate> _genEntityUpdate;
        private readonly Generator<EntityWorkCreate> _genEntityCreate;
        private readonly Generator<EntityWorkSave> _genEntitySave;
        private readonly Generator<AppInputTypes> _genInputType;
        private readonly Generator<ExportListXml> _exportListXmlGenerator;
        private readonly Generator<AppEntityRead> _genEntity;
        private readonly Generator<AppContentTypes> _genType;

        public AppWork(
            AppWorkContextService ctxSvc,
            Generator<AppEntityRead> genEntity,
            Generator<AppContentTypes> genType,
            Generator<AppInputTypes> genInputType,
            Generator<ExportListXml> exportListXmlGenerator,
            Generator<EntityWorkSave> genEntitySave,
            Generator<EntityWorkCreate> genEntityCreate,
            Generator<EntityWorkUpdate> genEntityUpdate,
            Generator<EntityWorkMetadata> genEntityMetadata,
            Generator<EntityWorkFieldList> genEntityFieldList,
            Generator<EntityWorkDelete> genEntityDelete,
            Generator<EntityWorkPublish> genEntityPublish) : base("App.SysCtF")
        {
            ConnectServices(
                CtxSvc = ctxSvc,
                _genEntity = genEntity,
                _genType = genType,
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

        public AppContentTypes ContentTypes => _typeRead.Get(() => _genType.New());
        private readonly GetOnce<AppContentTypes> _typeRead = new GetOnce<AppContentTypes>();

        public AppInputTypes InputTypes => _inputTypes.Get(() => _genInputType.New());
        private readonly GetOnce<AppInputTypes> _inputTypes = new GetOnce<AppInputTypes>();

        public ExportListXml EntityXmlExporter(IAppWorkCtx appCtx, string contentType)
            => _exportListXmlGenerator.New().Init(appCtx.AppState, appCtx.AppState.GetContentType(contentType));

        public AppEntityRead EntityRead() => _genEntity.New();

        public AppContentTypes ContentTypeRead() => _genType.New();


        public EntityWorkCreate EntityCreate(IAppWorkCtxWithDb appCtx) => _genEntityCreate.New().InitContext(appCtx);

        public EntityWorkSave EntitySave(IAppWorkCtxWithDb appCtx) => _genEntitySave.New().InitContext(appCtx);

        public EntityWorkSave EntitySave(AppState appState) => _genEntitySave.New().InitContext(CtxWithDb(appState));

        public EntityWorkSave EntitySave(AppState appState, DbDataController existingController) 
            => _genEntitySave.New().InitContext(CtxWithDb(appState, existingController));

        public EntityWorkUpdate EntityUpdate(IAppWorkCtxWithDb ctx, AppState appState = default) 
            => _genEntityUpdate.New().InitContext(ctx ?? CtxWithDb(appState));

        public EntityWorkMetadata EntityMetadata(IAppWorkCtxWithDb ctx, AppState appState = default) 
            => _genEntityMetadata.New().InitContext(ctx ?? CtxWithDb(appState));

        public EntityWorkFieldList EntityFieldList(IAppWorkCtxWithDb ctx = default, AppState appState = default)
            => _genEntityFieldList.New().InitContext(ctx ?? CtxWithDb(appState));

        public EntityWorkDelete EntityDelete(IAppWorkCtxWithDb ctx = default, AppState appState = default)
            => _genEntityDelete.New().InitContext(ctx ?? CtxWithDb(appState));

        public EntityWorkPublish EntityPublish(IAppWorkCtxWithDb ctx = default, AppState appState = default)
            => _genEntityPublish.New().InitContext(ctx ?? CtxWithDb(appState));

        #endregion
    }
}
