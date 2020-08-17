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

        public ITenant Tenant { get; }
        public IPage Page { get; protected set; }
        public IContainer Container { get; }
        public IUser User { get; }
    }
}
