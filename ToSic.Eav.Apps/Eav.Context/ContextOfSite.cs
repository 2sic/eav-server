using System;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Configuration;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context
{
    /// <summary>
    /// Context of site - fully DI compliant
    /// All these objects should normally be injectable
    /// In rare cases you may want to replace them, which is why Site/User have Set Accessors
    /// </summary>
    public class ContextOfSite: HasLog<IContextOfSite>, IContextOfSite
    {
        #region Constructor / DI

        public class ContextOfSiteDependencies
        {
            public IServiceProvider ServiceProvider { get; }
            public ISite Site { get; }
            public IUser User { get; }
            public Generator<AppPermissionCheck> AppPermissionCheckGenerator { get; }
            public Generator<IFeaturesInternal> FeaturesInternalGenerator { get; }

            public ContextOfSiteDependencies(IServiceProvider serviceProvider, 
                ISite site, 
                IUser user,
                Generator<AppPermissionCheck> appPermissionCheckGenerator,
                Generator<IFeaturesInternal> featuresInternalGenerator
                )
            {
                ServiceProvider = serviceProvider;
                Site = site;
                User = user;
                AppPermissionCheckGenerator = appPermissionCheckGenerator;
                FeaturesInternalGenerator = featuresInternalGenerator;
            }
        }

        public ContextOfSite(ContextOfSiteDependencies dependencies) : base("Eav.CtxSte")
        {
            Dependencies = dependencies;
            ServiceProvider = dependencies.ServiceProvider ?? throw new Exception("Context didn't receive service provider, but this is absolutely necessary.");
            Site = dependencies.Site;
            User = dependencies.User;
        }

        #endregion


        /// <inheritdoc />
        public ISite Site { get; set; }

        /// <inheritdoc />
        public IUser User { get; }

        /// <inheritdoc />
        public virtual bool UserMayEdit
        {
            get
            {
                var u = User;
                if (u == null) return false;
                return u.IsSuperUser || u.IsAdmin || u.IsDesigner;
            }
        }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public ContextOfSiteDependencies Dependencies { get; }

        /// <inheritdoc />
        public IContextOfSite Clone(ILog parentLog) => new ContextOfSite(Dependencies).Init(parentLog);
    }
}
