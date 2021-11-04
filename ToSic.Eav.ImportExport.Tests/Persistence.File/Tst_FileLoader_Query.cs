using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Repositories;
using ToSic.Testing.Shared;
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
            var loader = Build<FileSystemLoader>().Init(TestStorageRoot, RepositoryTypes.TestingDoNotUse, false, null, Log);
            IList<IEntity> cts;
            try
            {
                cts = loader.Queries();
            }
            finally
            {
                Trace.Write(Log.Dump());
            }
            return cts;
        }
    }
}
