using System.Diagnostics;
using Xunit.Abstractions;

namespace ToSic.Sys.Utils.Types;

/// <summary>
/// The tests
/// </summary>
/// <param name="output"></param>
public class TypeAttributeLookupTest(ITestOutputHelper output)
{
    #region Test Classes and Attribute

    /// <summary>
    /// The basic class without attribute
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Local
    private class WithoutAttribute;

    /// <summary>
    /// The decorated class
    /// </summary>
    [Test(ContentType = ExpectedName)]
    private class WithAttribute
    {
        public const string ExpectedName = "Something";
    }
    
    /// <summary>
    /// The attribute we're testing with
    /// </summary>
    private class TestAttribute : Attribute
    {
        public string? ContentType { get; set; }
    }

    #endregion


    [Fact]
    public void CheckWithoutAttribute()
    {
        var cache = new TypeAttributeLookup<string?>();

        var value = cache.Get<WithoutAttribute, TestAttribute>(a => a?.ContentType);

        Null(value);
    }

    [Fact]
    public void CheckWithAttribute()
    {
        var cache = new TypeAttributeLookup<string?>();

        var value = cache.Get<WithAttribute, TestAttribute>(a => a?.ContentType);

        Equal(WithAttribute.ExpectedName, value);
    }

    [Fact]
    public void CachingIsUsed()
    {
        var cache = new TypeAttributeLookup<string?>();

        cache.Get<WithAttribute, TestAttribute>(a => a?.ContentType);

        False(cache.UsedCache);
        cache.Get<WithAttribute, TestAttribute>(a => a?.ContentType);
        True(cache.UsedCache);
    }

    [Fact]
    public void CachingSpeedsUpBy25X()
    {
        // Create 100
        var caches = Enumerable.Range(0, 100)
            .Select(_ => new TypeAttributeLookup<string?>())
            .ToList();

        // Warm-up
        foreach (var cache in caches)
            cache.Get<WithoutAttribute, TestAttribute>(a => a?.ContentType);

        // Real usage - first should not be cached
        var first = Stopwatch.StartNew();
        foreach (var cache in caches)
            cache.Get<WithAttribute, TestAttribute>(a => a?.ContentType);
        first.Stop();

        var repeat = Stopwatch.StartNew();
        foreach (var cache in caches)
            cache.Get<WithAttribute, TestAttribute>(a => a?.ContentType);
        repeat.Stop();

        output.WriteLine($"First: {first.ElapsedTicks}");
        output.WriteLine($"Repeat: {repeat.ElapsedTicks}");
        // Note: it's usually 25+ times faster, but to avoid the test from failing under load, we only check for 3x here
        True(repeat.ElapsedTicks * 3 < first.ElapsedTicks);
    }

}
