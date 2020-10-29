using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public class InstanceContext: IInstanceContext
    {
        public InstanceContext(ISite site, IPage page, IContainer container, IUser user)
        {
            Tenant = site;
            Page = page;
            Container = container;
            User = user;
        }

        public IInstanceContext Clone(ISite site = null, IPage page = null, IContainer container = null, IUser user = null) 
            => new InstanceContext(site ?? Tenant, page ?? Page, container ?? Container, user ?? User);

        public ISite Tenant { get; }
        public IPage Page { get; protected set; }
        public IContainer Container { get; }
        public IUser User { get; }
    }
}
