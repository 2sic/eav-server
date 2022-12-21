﻿using ToSic.Eav.Apps.Security;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context
{
    /// <summary>
    /// Context of site - fully DI compliant
    /// All these objects should normally be injectable
    /// In rare cases you may want to replace them, which is why Site/User have Set Accessors
    /// </summary>
    public class ContextOfSite: HasLog, IContextOfSite
    {
        #region Constructor / DI

        public class Dependencies: ServiceDependencies
        {
            public ISite Site { get; }
            public IUser User { get; }
            public Generator<AppPermissionCheck> AppPermissionCheck { get; }

            public Dependencies(
                ISite site,
                IUser user,
                Generator<AppPermissionCheck> appPermissionCheck
            ) => AddToLogQueue(
                Site = site,
                User = user,
                AppPermissionCheck = appPermissionCheck
            );
        }

        public ContextOfSite(Dependencies dependencies) : base("Eav.CtxSte")
        {
            SiteDeps = dependencies.SetLog(Log);
            Site = dependencies.Site;
            User = dependencies.User;
        }

        #endregion


        /// <inheritdoc />
        public ISite Site { get; set; }

        /// <inheritdoc />
        public IUser User { get; }

        /// <inheritdoc />
        public virtual bool UserMayEdit => Log.GetAndLog<bool>(_ =>
        {
            var u = User;
            if (u == null) return false;
            return u.IsSystemAdmin || u.IsSiteAdmin || u.IsDesigner;
        });

        /// <inheritdoc />
        [PrivateApi]
        public Dependencies SiteDeps { get; }

        /// <inheritdoc />
        public IContextOfSite Clone(ILog parentLog) => new ContextOfSite(SiteDeps).Init(parentLog);
    }
}
