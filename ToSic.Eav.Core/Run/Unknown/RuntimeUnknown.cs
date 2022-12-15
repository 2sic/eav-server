using ToSic.Eav.Apps;
using ToSic.Lib.Logging;


namespace ToSic.Eav.Run.Unknown
{
    public class RuntimeUnknown: HasLog, IRuntime, IIsUnknown
    {
        public RuntimeUnknown(WarnUseOfUnknown<RuntimeUnknown> warn) : base("Eav.BscRnt") { }

        public AppState LoadFullAppState() => new AppState(new ParentAppState(null, false, false), Constants.PresetIdentity, Constants.PresetName, new Log(LogNames.NotImplemented));

        public void ReloadConfigEntities() { /* do nothing */ }
    }
}
