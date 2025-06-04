using ToSic.Lib.Services;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Lib.LookUp.Engines;

/// <summary>
/// Fall back implementation - just return an empty lookup engine
/// This should usually be 
/// </summary>
internal sealed class LookUpEngineResolverUnknown(WarnUseOfUnknown<LookUpEngineResolverUnknown> _) : ServiceBase($"{LogScopes.NotImplemented}.LookUp"), ILookUpEngineResolver, IIsUnknown
{
    public ILookUpEngine GetLookUpEngine(int moduleId) => new LookUpEngine(Log);
}