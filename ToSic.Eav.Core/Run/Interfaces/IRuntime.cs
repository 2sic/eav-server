using ToSic.Eav.Apps;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Run
{
    public interface IRuntime: IHasLog
    {
        AppState LoadFullAppState();
    }
}
