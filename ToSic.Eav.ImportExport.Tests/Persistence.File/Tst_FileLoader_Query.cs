using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Repositories;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\" + PathWith3Types, TestingPath3)]
    public class Tst_FileLoader_Query: PersistenceTestsBase
    {

        [TestMethod]
        public void FLoader_LoadQueriesAndCount()
        {
            var cts = LoadAllQueries();
            Assert.AreEqual(3, cts.Count, "test case has 3 content-types to deserialize");
        }
       
        


        private IList<IEntity> LoadAllQueries()
        {
            Trace.WriteLine($"path:'{TestStorageRoot}'");
            var loader = GetService<FileSystemLoader>().Init(Constants.PresetAppId, TestStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
            IList<IEntity> cts;
            try
            {
                cts = DirectEntitiesSource.Using(set => loader.Entities(FsDataConstants.QueriesFolder, 0, set.Source));
                //cts = new DirectEntitiesSource() loader.Entities(FsDataConstants.QueriesFolder, 0, new List<IEntity>());
            }
            finally
            {
                Trace.Write(Log.Dump());
            }
            return cts;
        }
    }
}
