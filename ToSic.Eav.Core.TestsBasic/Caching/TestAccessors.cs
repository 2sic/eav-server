using System.Runtime.Caching;
using ToSic.Sys.Caching.Policies;

namespace ToSic.Eav.Caching;

internal static class TestAccessors
{
    internal static CacheItemPolicy CreateResultTac(this IPolicyMaker policyMaker)
        => policyMaker.CreateResult();
}