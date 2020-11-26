using System;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public class RunContextCore: IRunContextCore
    {
        public RunContextCore(IServiceProvider serviceProvider, ISite site, IUser user)
        {
            ServiceProvider = serviceProvider ?? throw new Exception("Context didn't receive service provider, but this is absolutely necessary.");
            Site = site;
            User = user;
        }

        /// <inheritdoc />
        public ISite Site { get; set; }

        /// <inheritdoc />
        public IUser User { get; }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public IRunContextCore Clone() => new RunContextCore(ServiceProvider, Site, User);

    }
}
