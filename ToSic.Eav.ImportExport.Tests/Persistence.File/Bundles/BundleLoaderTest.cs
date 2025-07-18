﻿using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.Persistence.File;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File.Bundles;


public class BundleLoaderTest(ITestOutputHelper output, Generator<FileSystemLoader, FileSystemLoaderOptions> loaderGenerator) : ServiceBase("test"), IClassFixture<DoFixtureStartup<ScenarioMini>>
{
    [Fact]
    public void TypesInBundles()
    {
        var cts = LoadAllTypesInBundles();
        var someDataType = cts.FirstOrDefault(ct => ct.Name.Contains("SomeData"));
        NotNull(someDataType);//, "should find the SomeData type");
        Equal("Default", someDataType.Scope);//, "scope should be default");
    }

    [Fact]
    public void EntitiesInBundles()
    {
        var entities = LoadAllEntitiesInBundles();
        var someData = entities.FirstOrDefault(e => e.Type.Name.Contains("SomeData"));
        Equal(4, entities.Count);//, "test case has 4 entity in bundles to deserialize");
        Equal("fp638089655185445243", someData.GetTac("FirstProperty"));
    }

    [Fact]
    public void ExportConfiguration()
    {
        var entities = new LoaderHelper(PersistenceTestConstants.ScenarioMiniDeep, Log)
            .LoadAllQueryEntities(loaderGenerator, output);
        var systemExportConfiguration = entities.One(new System.Guid("22db39d7-8a59-43be-be68-ea0f28880c10"));
        NotNull(systemExportConfiguration);//, "should find the system export configuration");

        var export = new ExportConfiguration(systemExportConfiguration);
        Equal("cfg2", export.Name);//, "name should be cfg2");
        True(export.PreserveMarkers, "name should be cfg2");
    }

    private IList<IContentType> LoadAllTypesInBundles()
    {
        var testStorageRoot = TestFiles.GetTestPath(PersistenceTestConstants.ScenarioMiniDeep);
        var loader = loaderGenerator.New(new()
        {
            AppId = KnownAppsConstants.PresetAppId,
            Path = testStorageRoot,
            RepoType = RepositoryTypes.TestingDoNotUse,
            IgnoreMissing = false,
            EntitiesSource = null,
        });

        IList<IContentType> cts;
        try
        {
            cts = loader.ContentTypesInBundles()
                .Select(set => set.ContentType)
                .ToList();
        }
        finally
        {
            output.WriteLine(Log.Dump());
        }
        return cts;
    }
        
    private ICollection<IEntity> LoadAllEntitiesInBundles()
    {
        var testStorageRoot = TestFiles.GetTestPath(PersistenceTestConstants.ScenarioMiniDeep);
        output.WriteLine($"path:'{testStorageRoot}'");
        var loader = loaderGenerator.New(new()
        {
            AppId = KnownAppsConstants.PresetAppId,
            Path = testStorageRoot,
            RepoType = RepositoryTypes.TestingDoNotUse,
            IgnoreMissing = false,
            EntitiesSource = null
        });
        var relationshipsSource = new ImmutableEntitiesSource();
        try
        {
            return loader.EntitiesInBundles(relationshipsSource);
        }
        finally
        {
            output.WriteLine(Log.Dump());
        }
    }
        
}