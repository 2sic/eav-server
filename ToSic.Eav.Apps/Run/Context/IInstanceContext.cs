using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public interface IInstanceContext
    {
        ISite Tenant { get; }

        IPage Page { get; }

        IContainer Container { get; }

        IUser User { get; }

        IInstanceContext Clone(ISite site = null, IPage page = null, IContainer container = null, IUser user = null);
    }
}
