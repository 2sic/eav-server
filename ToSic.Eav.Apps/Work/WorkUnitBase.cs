using System;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Work
{
    /// <summary>
    /// Base class for all app-work helpers.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class WorkUnitBase<TContext> : ServiceBase where TContext : class, IAppWorkCtx
    {

        protected WorkUnitBase(string logName): base(logName)
        {
        }

        /// <summary>
        /// The current work context
        /// </summary>
        public TContext AppWorkCtx
        {
            get => _appWorkCtx ?? throw new Exception($"Can't use this before {nameof(AppWorkCtx)} is set - pls use a Work-Generator.");
            private set => _appWorkCtx = value;
        }
        private TContext _appWorkCtx;

        /// <summary>
        /// Internal init, should only ever be called by the GenWork... generators
        /// </summary>
        /// <param name="appWorkCtx"></param>
        internal void _initCtx(TContext appWorkCtx) => AppWorkCtx = appWorkCtx;
    }
}
