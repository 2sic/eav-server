using System;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    /// <summary>
    /// Context of site - fully DI compliant
    /// All these objects should normally be injectable
    /// In rare cases you may want to replace them, which is why Site/User have Set Accessors
    /// </summary>
    public class ContextOfSite: IContextOfSite
    {
        public ContextOfSite(IServiceProvider serviceProvider, ISite site, IUser user)
        {
            ServiceProvider = serviceProvider ?? throw new Exception("Context didn't receive service provider, but this is absolutely necessary.");
            Site = site;
            User = user;
        }

        /// <inheritdoc />
        public ISite Site { get; set; }

        /// <inheritdoc />
        public IUser User { get; set; }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public IContextOfSite Clone() => new ContextOfSite(ServiceProvider, Site, User);

    }
}
