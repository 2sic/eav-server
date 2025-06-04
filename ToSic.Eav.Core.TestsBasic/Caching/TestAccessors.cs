using System.Runtime.Caching;

namespace ToSic.Eav.Caching;

internal static class TestAccessors
{
    internal static CacheItemPolicy CreateResultTac(this IPolicyMaker policyMaker)
        => policyMaker.CreateResult();
}