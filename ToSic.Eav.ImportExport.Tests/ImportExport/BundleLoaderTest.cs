using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.Efc.Tests;
using ToSic.Eav.Repositories;
using ToSic.Lib.Logging;


// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests;

[TestClass]
public class BundleLoaderTest: Efc11TestBase
{
    /// <summary>
    /// Probably set at test-time?
    /// </summary>
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void TypesInBundles()
    {
        var cts = LoadAllTypesInBundles();
        var someDataType = cts.FirstOrDefault(ct => ct.Name.Contains("SomeData"));
        IsNotNull(someDataType, "should find the SomeData type");
        AreEqual("Default", someDataType.Scope, "scope should be default");
    }

    [TestMethod]
    public void EntitiesInBundles()
    {
        var entities = LoadAllEntitiesInBundles();
        var someData = entities.FirstOrDefault(e => e.Type.Name.Contains("SomeData"));
        AreEqual(4, entities.Count, "test case has 4 entity in bundles to deserialize");
        AreEqual("fp638089655185445243", someData.GetTac("FirstProperty"));
    }

    [TestMethod]
    public void ExportConfiguration()
    {
        var entities = LoadAllEntities();
        var systemExportConfiguration = entities.One(new System.Guid("22db39d7-8a59-43be-be68-ea0f28880c10"));
        IsNotNull(systemExportConfiguration, "should find the system export configuration");

        var export = new ExportConfiguration(systemExportConfiguration);
        AreEqual("cfg2", export.Name, "name should be cfg2");
        IsTrue(export.PreserveMarkers, "name should be cfg2");
    }

    private IList<IContentType> LoadAllTypesInBundles()
    {
        var testStorageRoot = PersistenceTestConstants.TestStorageRoot(TestContext);
        var loader = GetService<FileSystemLoader>()
            .Init(
                Constants.PresetAppId,
                testStorageRoot,
                RepositoryTypes.TestingDoNotUse,
                false,
                null
            );

        IList<IContentType> cts;
        try
        {
            cts = loader.ContentTypesInBundles()
                .Select(set => set.ContentType)
                .ToList();
        }
        finally
        {
            Trace.Write(Log.Dump());
        }
        return cts;
    }
        
    private List<IEntity> LoadAllEntitiesInBundles()
    {
        var testStorageRoot = PersistenceTestConstants.TestStorageRoot(TestContext);
        Trace.WriteLine($"path:'{testStorageRoot}'");
        var loader = GetService<FileSystemLoader>()
            .Init(Constants.PresetAppId, testStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
        var relationshipsSource = new ImmutableEntitiesSource();
        try
        {
            return loader.EntitiesInBundles(relationshipsSource);
        }
        finally
        {
            Trace.Write(Log.Dump());
        }
        return new List<IEntity>();
    }
        
    private IList<IEntity> LoadAllEntities()
    {
        var testStorageRoot = PersistenceTestConstants.TestStorageRoot(TestContext);
        Trace.WriteLine($"path:'{testStorageRoot}'");
        var loader = GetService<FileSystemLoader>().Init(Constants.PresetAppId, testStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
        IList<IEntity> entities;
        try
        {
            entities = DirectEntitiesSource.Using(set => loader.Entities(FsDataConstants.QueriesFolder, 0, set.Source));
        }
        finally
        {
            Trace.Write(Log.Dump());
        }
        return entities;
    }
}