using ToSic.Eav.Data;
using ToSic.Lib.Logging;

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
