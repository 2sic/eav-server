using ToSic.Lib.Data;

namespace ToSic.Lib.DI;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ISwitchableService : IHasIdentityNameId
{
    bool IsViable();

    int Priority { get; }
}