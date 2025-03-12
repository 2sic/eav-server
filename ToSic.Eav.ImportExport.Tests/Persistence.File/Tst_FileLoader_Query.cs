using System.Collections.Generic;
using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.Efc.Tests;
using ToSic.Eav.Repositories;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests;

[TestClass]
[DeploymentItem("..\\..\\" + PersistenceTestConstants.PathWith3Types, PersistenceTestConstants.TestingPath3)]
public class Tst_FileLoader_Query: Efc11TestBase
{
    /// <summary>
    /// Probably set at test-time?
    /// </summary>
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void FLoader_LoadQueriesAndCount()
    {
        var cts = LoadAllQueries();
        AreEqual(3, cts.Count, "test case has 3 content-types to deserialize");
    }
       
        


    private IList<IEntity> LoadAllQueries()
    {
        var testStorageRoot = PersistenceTestConstants.TestStorageRoot(TestContext);
        Trace.WriteLine($"path:'{testStorageRoot}'");
        var loader = GetService<FileSystemLoader>().Init(Constants.PresetAppId, testStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
        IList<IEntity> cts;
        try
        {
            cts = DirectEntitiesSource.Using(set => loader.Entities(FsDataConstants.QueriesFolder, 0, set.Source));
        }
        finally
        {
            Trace.Write(Log.Dump());
        }
        return cts;
    }
}