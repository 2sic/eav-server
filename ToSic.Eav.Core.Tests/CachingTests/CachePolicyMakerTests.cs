using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Caching;

namespace ToSic.Eav.Core.Tests.CachingTests;

[TestClass]
public class CachePolicyMakerTests
{
    private static CacheItemPolicyMaker Empty() => new(null, []);
    
    [TestMethod]
    public void CpmWithoutAnything()
        => Assert.IsNotNull(Empty().CreateResult());

    [TestMethod]
    public void CpmWithoutAnythingHasDefaultExpiration()
    {
        Assert.AreEqual(DateTimeOffset.MaxValue, Empty().CreateResult().AbsoluteExpiration);
        Assert.AreEqual(new(1, 0, 0), Empty().CreateResult().SlidingExpiration);
    }

    [TestMethod]
    public void CpmWithAbsoluteExpiration()
    {
        var exp = DateTimeOffset.Now;
        var result = Empty().SetAbsoluteExpiration(exp).CreateResult();
        Assert.AreEqual(exp, result.AbsoluteExpiration);
        Assert.AreEqual(TimeSpan.Zero, result.SlidingExpiration);
    }

    [TestMethod]
    public void CpmWithSlidingExpiration()
    {
        var exp = new TimeSpan(1, 0, 0);
        var result = Empty().SetSlidingExpiration(exp).CreateResult();
        Assert.AreEqual(DateTimeOffset.MaxValue, result.AbsoluteExpiration);
        Assert.AreEqual(exp, result.SlidingExpiration);
    }

    /// <summary>
    /// Note: According to the specs this is not allowed.
    /// But we won't catch this, since the developer should see the exception.
    /// </summary>
    [TestMethod]
    public void CmpWithBothSlidingAndAbsolute()
    {
        var abs = DateTimeOffset.Now.AddHours(1);
        var sld = new TimeSpan(1, 0, 0);
        var result = Empty().SetAbsoluteExpiration(abs).SetSlidingExpiration(sld).CreateResult();
        Assert.AreEqual(abs, result.AbsoluteExpiration);
        Assert.AreEqual(sld, result.SlidingExpiration);
    }
}