﻿using System.Diagnostics;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Repositories;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;
using Xunit.Abstractions;
using EavDbContext = ToSic.Eav.Persistence.Efc.Sys.DbContext.EavDbContext;

namespace ToSic.Eav.Persistence.Efc.Tests;

[Startup(typeof(StartupTestsApps))]
public class Efc11LoadTests(EavDbContext db, Generator<EfcAppLoaderService> loaderGenerator, ITestOutputHelper output)
    : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    /// <summary>
    /// AppId of the Blog App in the eav-testing DB
    /// </summary>
    private const int BlogAppId = ScenarioConstants.AppBlogId;
    private const int ExpectedContentTypesOnBlog = ScenarioConstants.AppBlogOwnContentTypeCount;

    [field: System.Diagnostics.CodeAnalysis.AllowNull, System.Diagnostics.CodeAnalysis.MaybeNull]
    private EfcAppLoaderService Loader => field ??= loaderGenerator.New().UseExistingDb(db);

    [Fact]
    public void GetSomething()
    {
        var results = db.TsDynDataZones.Single(z => z.ZoneId == 1);
        True(results.ZoneId == 1, "zone doesn't fit - it is " + results.ZoneId);
    }

    [Fact]
    public void BlogAppHasAround350Entities()
    {
        var results = Loader.AppStateReaderRawTac(BlogAppId);
        InRange(results.List.Count, 350, 370);
    }

    [Theory]
    [InlineData(25)]
    public void PerformanceLoadingBlog25X(int loadCount)
    {
        var x = Stopwatch.StartNew();
        for (var i = 0; i < loadCount; i++)
        {
            var loader = loaderGenerator.New().UseExistingDb(db);
            loader.AppStateReaderRawTac(BlogAppId);
        }

        output.WriteLine($"Loading {loadCount} times took {x.ElapsedMilliseconds}ms");
    }


    [Fact]
    public void LoadContentTypesOfBlogOnce()
    {
        var x = Stopwatch.StartNew();
        var results = TestLoadCts(BlogAppId);
        Equal(ExpectedContentTypesOnBlog, results.Count);//, "dummy test: ");
        output.WriteLine($"Loading took {x.ElapsedMilliseconds}ms");
    }

    [Theory]
    [InlineData(25)]
    public void LoadContentTypesOfBlogTen(int max)
    {
        var timer = Stopwatch.StartNew();
        var results = TestLoadCts(BlogAppId);
        for (var x = 0; x < max; x++)
            results = TestLoadCts(BlogAppId);
        Equal(ExpectedContentTypesOnBlog, results.Count);//, "dummy test: ");
        output.WriteLine($"Loading {max} times took {timer.ElapsedMilliseconds}ms");
    }

    private ICollection<IContentType> TestLoadCts(int appId)
        => (Loader as IAppsAndZonesLoader).ContentTypes(appId, null);
}