using ToSic.Eav.Apps;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Run
{
    public interface IRuntime: IHasLog
    {
        AppState LoadFullAppState();
    }
}
