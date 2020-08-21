using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public class InstanceContext: IInstanceContext
    {
        public InstanceContext(ITenant tenant, IPage page, IContainer container, IUser user)
        {
            Tenant = tenant;
            Page = page;
            Container = container;
            User = user;
        }

        public IInstanceContext Clone(ITenant tenant = null, IPage page = null, IContainer container = null, IUser user = null) 
            => new InstanceContext(tenant ?? Tenant, page ?? Page, container ?? Container, user ?? User);

        public ITenant Tenant { get; }
        public IPage Page { get; protected set; }
        public IContainer Container { get; }
        public IUser User { get; }
    }
}
