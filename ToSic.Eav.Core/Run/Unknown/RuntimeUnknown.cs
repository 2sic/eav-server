using ToSic.Eav.Apps;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;


namespace ToSic.Eav.Run.Unknown
{
    public class RuntimeUnknown: ServiceBase, IRuntime, IIsUnknown
    {
        public RuntimeUnknown(WarnUseOfUnknown<RuntimeUnknown> _) : base("Eav.BscRnt") { }

        public AppState LoadFullAppState() => new AppState(new ParentAppState(null, false, false), Constants.PresetIdentity, Constants.PresetName, new Log(LogScopes.NotImplemented));

        public void ReloadConfigEntities() { /* do nothing */ }
    }
}
