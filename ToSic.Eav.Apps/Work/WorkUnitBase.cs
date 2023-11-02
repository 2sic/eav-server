using System;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Work
{
    public abstract class WorkUnitBase<TContext> : ServiceBase where TContext : class, IAppWorkCtx
    {
        private TContext _appWorkCtx;

        protected WorkUnitBase(string logName) : base(logName)
        {
        }

        public TContext AppWorkCtx
        {
            get => _appWorkCtx ?? throw new Exception($"Can't use this before {nameof(AppWorkCtx)} is set using {nameof(WorkWithContextBaseInit.InitContext)}.");
            private set => _appWorkCtx = value;
        }

        internal void _initCtx(TContext appWorkCtx) => AppWorkCtx = appWorkCtx;
    }

    public static class WorkWithContextBaseInit
    {
        public static TWork InitContext<TWork, TContext>(this TWork original, TContext appWorkContext)
            where TContext : class, IAppWorkCtx
            where TWork : WorkUnitBase<TContext>
        {
            original._initCtx(appWorkContext);
            return original;
        }
    }
}
