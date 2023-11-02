using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Apps.Parts;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Work
{
    /// <summary>
    /// Helper to generate a App Work Unit class to get a single / simple thing done.
    ///
    /// Primarily used to do one single operation such as a simple delete, publish etc.
    /// </summary>
    /// <typeparam name="TWork"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public class AppWorkUnit<TWork, TContext>: ServiceBase
        where TWork : AppWorkBase<TContext>
        where TContext : class, IAppWorkCtx
    {
        private readonly AppWorkContextService _ctxSvc;
        private readonly Generator<TWork> _generator;

        public AppWorkUnit(AppWorkContextService ctxSvc, Generator<TWork> generator): base("App.WorkUn")
        {
            ConnectServices(
                _ctxSvc = ctxSvc,
                _generator = generator
            );
        }

        public TWork New(IAppWorkCtx ctx = default, IAppIdentity identity = default, AppState state = default, int? appId = default) 
            => _generator.New().InitContext(_ctxSvc.CtxGen<TContext>(ctx, identity, state, appId));
    }
}
