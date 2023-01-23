using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Repositories;
using ToSic.Eav.WebApi.ImportExport;
using ToSic.Lib.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests
{
    [TestClass]
    public class BundleLoaderTest: PersistenceTestsBase
    {
        [TestMethod]
        public void TypesInBundles()
        {
            var cts = LoadAllTypesInBundles();
            var someDataType = cts.FirstOrDefault(ct => ct.Name.Contains("SomeData"));
            Assert.IsNotNull(someDataType, "should find the SomeData type");
            Assert.AreEqual("Default", someDataType.Scope, "scope should be default");
        }

        [TestMethod]
        public void EntitiesInBundles()
        {
            var entities = LoadAllEntitiesInBundles();
            var someData = entities.FirstOrDefault(e => e.Type.Name.Contains("SomeData"));
            Assert.AreEqual(4, entities.Count, "test case has 4 entity in bundles to deserialize");
            Assert.AreEqual("fp638089655185445243", someData.Value("FirstProperty"));
        }

        [TestMethod]
        public void ExportConfiguration()
        {
            var entities = LoadAllEntities();
            var systemExportConfiguration = entities.One(new System.Guid("22db39d7-8a59-43be-be68-ea0f28880c10"));
            Assert.IsNotNull(systemExportConfiguration, "should find the system export configuration");

            var export = new ExportConfiguration(systemExportConfiguration);
            Assert.AreEqual("cfg2", export.Name, "name should be cfg2");
            Assert.IsTrue(export.PreserveMarkers, "name should be cfg2");
        }

        private IList<IContentType> LoadAllTypesInBundles()
        {
            var loader = Build<FileSystemLoader>()
                .Init(Constants.PresetAppId, TestStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
            IList<IContentType> cts;
            try
            {
                cts = loader.ContentTypesInBundles();
            }
            finally
            {
                Trace.Write(Log.Dump());
            }
            return cts;
        }
        
        private IList<Entity> LoadAllEntitiesInBundles()
        {
            Trace.WriteLine($"path:'{TestStorageRoot}'");
            var loader = Build<FileSystemLoader>().Init(Constants.PresetAppId, TestStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
            var relationshipsSource = new DirectEntitiesSource(new List<IEntity>());
            IList<Entity> entities;
            try
            {
                entities = loader.EntitiesInBundles(relationshipsSource);
            }
            finally
            {
                Trace.Write(Log.Dump());
            }
            return entities;
        }
        
        private IList<IEntity> LoadAllEntities()
        {
            Trace.WriteLine($"path:'{TestStorageRoot}'");
            var loader = Build<FileSystemLoader>().Init(Constants.PresetAppId, TestStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
            IList<IEntity> entities;
            try
            {
                entities = loader.Entities(FsDataConstants.QueriesFolder, 0); // .Queries(0);
            }
            finally
            {
                Trace.Write(Log.Dump());
            }
            return entities;
        }
    }
}
