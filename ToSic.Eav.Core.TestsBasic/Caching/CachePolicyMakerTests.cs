using ToSic.Lib.Logging;

namespace ToSic.Eav.Caching;

public class CachePolicyMakerTests
{
    /// <summary>
    /// Empty Test Constructor
    /// </summary>
    private static CacheItemPolicyMaker EmptyTco() => new() { Log = new Log("Tst.CacSpx") };
    
    [Fact]
    public void CpmWithoutAnything()
        => NotNull(EmptyTco().CreateResultTac());

    [Fact]
    public void CpmWithoutAnythingHasDefaultExpiration()
    {
        Equal(DateTimeOffset.MaxValue, EmptyTco().CreateResultTac().AbsoluteExpiration);
        Equal(new(1, 0, 0), EmptyTco().CreateResultTac().SlidingExpiration);
    }

    [Fact]
    public void CpmWithAbsoluteExpiration()
    {
        var exp = DateTimeOffset.Now;
        var result = EmptyTco().SetAbsoluteExpiration(exp).CreateResultTac();
        Equal(exp, result.AbsoluteExpiration);
        Equal(TimeSpan.Zero, result.SlidingExpiration);
    }

    [Fact]
    public void CpmWithSlidingExpiration()
    {
        var exp = new TimeSpan(1, 0, 0);
        var result = EmptyTco().SetSlidingExpiration(exp).CreateResultTac();
        Equal(DateTimeOffset.MaxValue, result.AbsoluteExpiration);
        Equal(exp, result.SlidingExpiration);
    }

    /// <summary>
    /// Note: According to the specs this is not allowed.
    /// But we won't catch this, since the developer should see the exception.
    /// </summary>
    [Fact]
    public void CmpWithBothSlidingAndAbsolute()
    {
        var abs = DateTimeOffset.Now.AddHours(1);
        var sld = new TimeSpan(1, 0, 0);
        var result = EmptyTco().SetAbsoluteExpiration(abs).SetSlidingExpiration(sld).CreateResultTac();
        Equal(abs, result.AbsoluteExpiration);
        Equal(sld, result.SlidingExpiration);
    }
}