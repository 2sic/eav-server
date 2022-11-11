using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Configuration;
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
            Assert.AreEqual(2, cts.Count, "test case has 3 content-types to deserialize");
        }
       
        


        private IList<IEntity> LoadAllQueries()
        {
            Trace.WriteLine($"path:'{TestStorageRoot}'");
            var loader = Build<FileSystemLoader>().Init(Constants.PresetAppId, TestStorageRoot, RepositoryTypes.TestingDoNotUse, false, null, Log);
            IList<IEntity> cts;
            try
            {
                cts = loader.Entities(FsDataConstants.QueriesFolder, 0); // .Queries(0);
            }
            finally
            {
                Trace.Write(Log.Dump());
            }
            return cts;
        }
    }
}
