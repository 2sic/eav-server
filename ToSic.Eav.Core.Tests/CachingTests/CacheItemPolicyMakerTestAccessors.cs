using System.Runtime.Caching;
using ToSic.Eav.Caching;

namespace ToSic.Eav.Core.Tests.CachingTests;

internal static class CacheItemPolicyMakerTestAccessors
{
    internal static CacheItemPolicy CreateResultTac(this IPolicyMaker policyMaker)
        => policyMaker.CreateResult();
}