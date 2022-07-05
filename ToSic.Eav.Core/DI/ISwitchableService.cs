using ToSic.Eav.Data;

namespace ToSic.Eav.DI
{
    public interface ISwitchableService: IHasIdentityNameId
    {
        bool IsViable();

        int Priority { get; }
    }
}
