﻿using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.LookUp;

/// <summary>
/// Fall back implementation - just return an empty lookup engine
/// This should usually be 
/// </summary>
internal sealed class LookUpEngineResolverUnknown: ServiceBase, ILookUpEngineResolver, IIsUnknown
{
    public LookUpEngineResolverUnknown(WarnUseOfUnknown<LookUpEngineResolverUnknown> _) : base($"{LogScopes.NotImplemented}.LookUp")
    {
    }

    public ILookUpEngine GetLookUpEngine(int moduleId) => new LookUpEngine(Log);
}