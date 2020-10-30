using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public class InstanceContext: IInstanceContext
    {
        public InstanceContext(ISite site, IPage page, IContainer container, IUser user, IServiceProvider serviceProvider)
        {
            Tenant = site;
            Page = page;
            Container = container;
            User = user;
            _serviceProvider = serviceProvider;
        }

        public IInstanceContext Clone(ISite site = null, IPage page = null, IContainer container = null, IUser user = null) 
            => new InstanceContext(site ?? Tenant, page ?? Page, container ?? Container, user ?? User, ServiceProvider);

        [PrivateApi("TempTempCulture/wip")]
        public IServiceProvider ServiceProvider => _serviceProvider ?? (_serviceProvider = Eav.Factory.GetServiceProvider());
        private IServiceProvider _serviceProvider;

        public ISite Tenant { get; }
        public IPage Page { get; protected set; }
        public IContainer Container { get; }
        public IUser User { get; }
    }
}
