using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public interface IInstanceContext
    {
        ITenant Tenant { get; }

        IPage Page { get; }

        IContainer Container { get; }

        IUser User { get; }
    }
}
