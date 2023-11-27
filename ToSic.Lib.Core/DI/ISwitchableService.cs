using ToSic.Lib.Data;

namespace ToSic.Lib.DI;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ISwitchableService : IHasIdentityNameId
{
    bool IsViable();

    int Priority { get; }
}