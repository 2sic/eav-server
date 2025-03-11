﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;
using Xunit.Abstractions;

namespace ToSic.Eav.Persistence.Efc.Tests19;

[Startup(typeof(StartupTestFullWithDb))]
public class Efc11LoadTests(EavDbContext db, Generator<EfcAppLoader> loaderGenerator, ITestOutputHelper output)
    : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    /// <summary>
    /// AppId of the Blog App in the eav-testing DB
    /// </summary>
    private const int BlogAppId = 5260;
    private const int ExpectedContentTypesOnBlog = 7;

#if NETCOREAPP
    [field: AllowNull, MaybeNull]
#endif
    private EfcAppLoader Loader => field ??= loaderGenerator.New().UseExistingDb(db);

    [Fact]
    public void GetSomething()
    {
        var results = db.ToSicEavZones.Single(z => z.ZoneId == 1);
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

    [Fact]
    public void LoadContentTypesOf2TenXCached()
    {
        var results = TestLoadCts(BlogAppId);
        for (var x = 0; x < 9; x++)
            results = TestLoadCts(BlogAppId);
        Equal(ExpectedContentTypesOnBlog, results.Count);//, "dummy test: ");
    }

    [Fact]
    public void LoadContentTypesOf2TenXCleared()
    {
        var results = TestLoadCts(BlogAppId);
        for (var x = 0; x < 9; x++)
        {
            results = TestLoadCts(BlogAppId);
        }
        Equal(ExpectedContentTypesOnBlog, results.Count);//, "dummy test: ");
    }

    private IList<IContentType> TestLoadCts(int appId) => (Loader as IRepositoryLoader).ContentTypes(appId, null);
}