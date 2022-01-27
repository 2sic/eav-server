using ToSic.Eav.Apps;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Run
{
    public interface IRuntime: IHasLog<IRuntime>
    {
        AppState LoadFullAppState();

        //void ReloadConfigEntities();
    }
}
