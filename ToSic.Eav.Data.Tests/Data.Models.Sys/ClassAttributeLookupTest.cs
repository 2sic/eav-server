using System.Diagnostics;
using ToSic.Eav.Models;
using ToSic.Eav.Models.Sys;
using Xunit.Abstractions;

namespace ToSic.Eav.Data.Models.Sys;

/// <summary>
/// The basic model without decorators
/// </summary>
public class ModelWithoutDecorator;

/// <summary>
/// The decorated model
/// </summary>
[ModelSpecs(ContentType = ExpectedName)]
public class ModelWithDecorator
{
    public const string ExpectedName = "Something";
}

/// <summary>
/// The tests
/// </summary>
/// <param name="output"></param>
public class ClassAttributeLookupTest(ITestOutputHelper output)
{
    [Fact]
    public void CheckWithoutAttribute()
    {
        var cache = new ClassAttributeLookup<string?>();

        var value = cache.Get<ModelWithoutDecorator, ModelSpecsAttribute>(a => a?.ContentType);

        Null(value);
    }

    [Fact]
    public void CheckWithAttribute()
    {
        var cache = new ClassAttributeLookup<string?>();

        var value = cache.Get<ModelWithDecorator, ModelSpecsAttribute>(a => a?.ContentType);

        Equal(ModelWithDecorator.ExpectedName, value);
    }

    [Fact]
    public void CachingIsUsed()
    {
        var cache = new ClassAttributeLookup<string?>();

        cache.Get<ModelWithDecorator, ModelSpecsAttribute>(a => a?.ContentType);

        False(cache.UsedCache);
        cache.Get<ModelWithDecorator, ModelSpecsAttribute>(a => a?.ContentType);
        True(cache.UsedCache);
    }

    [Fact]
    public void CachingSpeedsUpBy25X()
    {
        // Create 100
        var caches = Enumerable.Range(0, 100)
            .Select(_ => new ClassAttributeLookup<string?>())
            .ToList();

        // Warm-up
        foreach (var cache in caches)
            cache.Get<ModelWithoutDecorator, ModelSpecsAttribute>(a => a?.ContentType);

        // Real usage - first should not be cached
        var first = Stopwatch.StartNew();
        foreach (var cache in caches)
            cache.Get<ModelWithDecorator, ModelSpecsAttribute>(a => a?.ContentType);
        first.Stop();

        var repeat = Stopwatch.StartNew();
        foreach (var cache in caches)
            cache.Get<ModelWithDecorator, ModelSpecsAttribute>(a => a?.ContentType);
        repeat.Stop();

        output.WriteLine($"First: {first.ElapsedTicks}");
        output.WriteLine($"Repeat: {repeat.ElapsedTicks}");
        True(repeat.ElapsedTicks * 25 < first.ElapsedTicks);
    }

}
