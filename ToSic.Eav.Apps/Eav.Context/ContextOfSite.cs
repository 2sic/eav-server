using System;
using ToSic.Eav.Logging;

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

        public ContextOfSite(IServiceProvider serviceProvider, ISite site, IUser user): base("Eav.CtxSte")
        {
            ServiceProvider = serviceProvider ?? throw new Exception("Context didn't receive service provider, but this is absolutely necessary.");
            Site = site;
            User = user;
        }

        public IContextOfSite Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }

        #endregion


        /// <inheritdoc />
        public ISite Site { get; set; }

        /// <inheritdoc />
        public IUser User { get; set; }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public IContextOfSite Clone(ILog parentLog) => new ContextOfSite(ServiceProvider, Site, User).Init(parentLog);
    }
}
