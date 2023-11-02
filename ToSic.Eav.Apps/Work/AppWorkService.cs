using ToSic.Eav.Apps.AppSys;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment
#pragma warning disable IDE0074

namespace ToSic.Eav.Apps.Work
{
    /// <summary>
    /// This service helps do all kinds of work on an app.
    /// It manages the work context and shares it between all the smaller services
    /// which are cached.
    ///
    /// Note that this is great for a lot of operations,
    /// but in rare cases you need to regenerate the state or do more fine-tuned work
    /// in which case you should just use the <see cref="AppWork"/> instead. 
    /// </summary>
    public class AppWorkService : WorkUnitBase<IAppWorkCtx>
    {
        #region Constructor, Init, Context

        /// <summary>
        /// The constructor for DI.
        /// Note that you must always call <see cref="WorkWithContextBaseInit.InitContext" /> for everything to work.
        /// </summary>
        /// <param name="appWork"></param>
        public AppWorkService(AppWork appWork) : base("App.WrkSvc")
        {
            AppWork = appWork;
        }

        /// <summary>
        /// Standard init - just with App State and everything else as default.
        /// </summary>
        /// <returns></returns>
        public AppWorkService Init(AppState appState) => this.InitContext(AppWork.CtxSvc.Context(appState));

        /// <summary>
        /// Init using ID and optionally setting showDrafts.
        /// </summary>
        /// <returns></returns>
        public AppWorkService Init(int appId, bool? showDrafts)
        {
            _workCtxPlus = AppWork.ContextPlus(appId, showDrafts);
            Init(_workCtxPlus.AppState);
            return this;
        }
        /// <summary>
        /// The underlying appWork
        /// </summary>
        public AppWork AppWork { get; }

        public AppState AppState => AppWorkCtx.AppState;
        public int AppId => AppWorkCtx.AppId;

        public IAppWorkCtx WorkCtx => AppWorkCtx;
        public IAppWorkCtxPlus WorkCtxPlus => _workCtxPlus ?? (_workCtxPlus = AppWork.ToCtxPlus(AppWorkCtx));
        private IAppWorkCtxPlus _workCtxPlus;

        public IAppWorkCtxWithDb WorkCtxWithDb => _workCtxWithDb ?? (_workCtxWithDb = AppWork.CtxWithDb(AppWorkCtx.AppState));
        private IAppWorkCtxWithDb _workCtxWithDb;

        #endregion

        public AppEntityRead Entities => _entRead ?? (_entRead = AppWork.Entities);
        private AppEntityRead _entRead;


        public WorkEntityCreate EntityCreate => _create ?? (_create = AppWork.EntityCreate(WorkCtxWithDb));
        private WorkEntityCreate _create;

        public WorkEntityDelete EntityDelete => _del ?? (_del = AppWork.EntityDelete(WorkCtxWithDb));
        private WorkEntityDelete _del;

        public WorkEntityUpdate EntityUpdate => _update ?? (_update = AppWork.EntityUpdate(WorkCtxWithDb));
        private WorkEntityUpdate _update;

        public WorkEntitySave EntitySave => _save ?? (_save = AppWork.EntitySave(WorkCtxWithDb));
        private WorkEntitySave _save;

        public WorkFieldList FieldList => _fieldList ?? (_fieldList = AppWork.EntityFieldList(WorkCtxWithDb));
        private WorkFieldList _fieldList;

        public WorkMetadata Metadata => _metadata ?? (_metadata = AppWork.EntityMetadata(WorkCtxWithDb));
        private WorkMetadata _metadata;

        public WorkEntityPublish Publish => _publish ?? (_publish = AppWork.EntityPublish(WorkCtxWithDb));
        private WorkEntityPublish _publish;
    }
}
