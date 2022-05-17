using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Run
{
    /// <summary>
    /// WIP
    /// </summary>
    public interface IStartUpRegistrations: IHasLog, IHasIdentityNameId
    {
        void Register();
    }
}
