using ToSic.Lib.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// Fall back implementation - just return an empty lookup engine
    /// This should usually be 
    /// </summary>
    public sealed class LookUpEngineResolverUnknown: HasLog, ILookUpEngineResolver, IIsUnknown
    {
        public LookUpEngineResolverUnknown(WarnUseOfUnknown<LookUpEngineResolverUnknown> warn) : base($"{LogScopes.NotImplemented}.LookUp")
        {
        }

        public ILookUpEngine GetLookUpEngine(int moduleId) => new LookUpEngine(Log);
    }
}
