using ToSic.Eav.Apps;
using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Loaders
{
    public class AppLoaderUnknown: ServiceBase, IAppLoader, IIsUnknown
    {
        public AppLoaderUnknown(WarnUseOfUnknown<AppLoaderUnknown> _) : base("Eav.BscRnt") { }

        public AppState LoadFullAppState() => new(new ParentAppState(null, false, false), Constants.PresetIdentity, Constants.PresetName, new Log(LogScopes.NotImplemented));

        public void ReloadConfigEntities() { /* do nothing */ }
    }
}
