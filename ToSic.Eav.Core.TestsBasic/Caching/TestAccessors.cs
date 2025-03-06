using System.Runtime.Caching;
using ToSic.Eav.Caching;

namespace ToSic.Eav.Tests.Caching;

internal static class TestAccessors
{
    internal static CacheItemPolicy CreateResultTac(this IPolicyMaker policyMaker)
        => policyMaker.CreateResult();
}