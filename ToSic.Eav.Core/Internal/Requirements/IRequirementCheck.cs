using ToSic.Lib.DI;

namespace ToSic.Eav.Configuration
{
    public interface IRequirementCheck: ISwitchableService
    {
        bool IsOk(Condition condition);

        string InfoIfNotOk(Condition condition);
    }
}
