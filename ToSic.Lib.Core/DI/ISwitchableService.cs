using ToSic.Lib.Data;

namespace ToSic.Lib.DI
{
    public interface ISwitchableService : IHasIdentityNameId
    {
        bool IsViable();

        int Priority { get; }
    }
}
