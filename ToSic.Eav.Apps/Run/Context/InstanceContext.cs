using System;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public class InstanceContext: IInstanceContext
    {
        public InstanceContext(ISite site, IPage page, IContainer container, IUser user, IServiceProvider serviceProvider,
            InstancePublishingState publishing)
        {
            Tenant = site;
            Page = page;
            Container = container;
            User = user;
            ServiceProvider = serviceProvider ?? throw new Exception("Context didn't receive service provider, but this is absolutely necessary.");
            Publishing = publishing;
        }


        public ISite Tenant { get; }
        public IPage Page { get; protected set; }
        public IContainer Container { get; }
        public IUser User { get; }
        public IServiceProvider ServiceProvider { get; }
        public InstancePublishingState Publishing { get; }

        public IInstanceContext Clone(ISite site = null, IPage page = null, IContainer container = null, IUser user = null) 
            => new InstanceContext(site ?? Tenant, page ?? Page, container ?? Container, user ?? User, ServiceProvider, Publishing);

    }
}
