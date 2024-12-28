using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Caching;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Core.Tests.CachingTests;

[TestClass]
public class CachePolicyMakerTests
{
    private static CacheItemPolicyMaker Empty() => new() { Log = new Log(CacheItemPolicyMaker.LogName) };
    
    [TestMethod]
    public void CpmWithoutAnything()
        => Assert.IsNotNull(Empty().CreateResultTac());

    [TestMethod]
    public void CpmWithoutAnythingHasDefaultExpiration()
    {
        Assert.AreEqual(DateTimeOffset.MaxValue, Empty().CreateResultTac().AbsoluteExpiration);
        Assert.AreEqual(new(1, 0, 0), Empty().CreateResultTac().SlidingExpiration);
    }

    [TestMethod]
    public void CpmWithAbsoluteExpiration()
    {
        var exp = DateTimeOffset.Now;
        var result = Empty().SetAbsoluteExpiration(exp).CreateResultTac();
        Assert.AreEqual(exp, result.AbsoluteExpiration);
        Assert.AreEqual(TimeSpan.Zero, result.SlidingExpiration);
    }

    [TestMethod]
    public void CpmWithSlidingExpiration()
    {
        var exp = new TimeSpan(1, 0, 0);
        var result = Empty().SetSlidingExpiration(exp).CreateResultTac();
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
        var result = Empty().SetAbsoluteExpiration(abs).SetSlidingExpiration(sld).CreateResultTac();
        Assert.AreEqual(abs, result.AbsoluteExpiration);
        Assert.AreEqual(sld, result.SlidingExpiration);
    }
}