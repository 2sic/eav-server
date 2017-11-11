using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    public class SqlDataSource_Test
    {
        private const string ConnectionDummy = "";
        private const string ConnectionName = "Data Source=.\\SQLExpress;Initial Catalog=2flex 2Sexy Content;Integrated Security=True";
        private const string ContentTypeName = "SqlData";

        [TestMethod]
        public void SqlDataSource_NoConfigChangesIfNotNecessary()
        {
            var initQuery = "Select * From Products";
            var sql = GenerateSqlDataSource(ConnectionDummy, initQuery, ContentTypeName);
            var configCountBefore = sql.Configuration.Count;
            sql.EnsureConfigurationIsLoaded();

            Assert.AreEqual(configCountBefore, sql.Configuration.Count);
            Assert.AreEqual(initQuery, sql.SelectCommand);
        }

        #region test parameter injection
        [TestMethod]
        public void SqlDataSource_SqlInjectionProtection()
        {
            var initQuery = "Select * From Products Where ProductId = [QueryString:Id]";
            var expectedQuery = "Select * From Products Where ProductId = @" + SqlDataSource.ExtractedParamPrefix + "1";
            var sql = GenerateSqlDataSource(ConnectionDummy, initQuery, ContentTypeName);
            var configCountBefore = sql.Configuration.Count;
            sql.EnsureConfigurationIsLoaded();

            Assert.AreEqual(configCountBefore + 1, sql.Configuration.Count);
            Assert.AreEqual(expectedQuery, sql.SelectCommand);
            Assert.AreEqual("", sql.Configuration["@" + SqlDataSource.ExtractedParamPrefix + "1"]);
        }

        [TestMethod]
        public void SqlDataSource_SqlInjectionProtectionComplex()
        {
            var initQuery = @"Select Top [AppSettings:MaxPictures] * 
From Products 
Where CatName = [AppSettings:DefaultCategoryName||NotFound] 
And ProductSort = [AppSettings:DoesntExist||CorrectlyDefaulted]";
            var expectedQuery = @"Select Top @" + SqlDataSource.ExtractedParamPrefix + @"1 * 
From Products 
Where CatName = @" + SqlDataSource.ExtractedParamPrefix + @"2 
And ProductSort = @" + SqlDataSource.ExtractedParamPrefix + @"3";

            var sql = GenerateSqlDataSource(ConnectionDummy, initQuery, ContentTypeName);
            var configCountBefore = sql.Configuration.Count;
            sql.EnsureConfigurationIsLoaded();

            Assert.AreEqual(configCountBefore + 3, sql.Configuration.Count);
            Assert.AreEqual(expectedQuery, sql.SelectCommand);
            Assert.AreEqual(ValueProvider.ValueCollectionProvider_Test.MaxPictures, sql.Configuration["@" + SqlDataSource.ExtractedParamPrefix + "1"]);
            Assert.AreEqual(ValueProvider.ValueCollectionProvider_Test.DefaultCategory, sql.Configuration["@" + SqlDataSource.ExtractedParamPrefix + "2"]);
            Assert.AreEqual("CorrectlyDefaulted", sql.Configuration["@" + SqlDataSource.ExtractedParamPrefix + "3"]);
        }

        #endregion

        #region test bad sql statements like insert / drop etc.
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlDataSource_BadSqlInsert() 
            => GenerateSqlDataSource(ConnectionDummy, "Insert something into something", ContentTypeName).LightList.Any();

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlDataSource_BadSqlSelectInsert() 
            => GenerateSqlDataSource(ConnectionDummy, "Select * from table; Insert something", ContentTypeName).LightList.Any();

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlDataSource_BadSqlSpaceInsert() 
            => GenerateSqlDataSource(ConnectionDummy, " Insert something into something", ContentTypeName).LightList.Any();

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlDataSource_BadSqlDropInsert() 
            => GenerateSqlDataSource(ConnectionDummy, "Drop tablename", ContentTypeName).LightList.Any();

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlDataSource_BadSqlSelectDrop() 
            => GenerateSqlDataSource(ConnectionDummy, "Select * from Products; Drop tablename", ContentTypeName).LightList.Any();

        #endregion


        #region test title / entityid fields with casing

        [TestMethod]
        public void SqlDataSource_TitleCasing()
        {
            var select = "SELECT [PortalID] as entityId, HomeDirectory As entityTitle, " +
                         "[AdministratorId],[GUID],[HomeDirectory],[PortalGroupID] " +
                         "FROM [Portals]";
            var sql = GenerateSqlDataSource(ConnectionName, select, ContentTypeName);
            var list = sql.LightList;
            Assert.IsTrue(list.Any(), "found some");
        }

        #endregion

        public static SqlDataSource GenerateSqlDataSource(string connection, string query, string typeName)
        {
            var source = new SqlDataSource(connection, query, typeName);
            source.ConfigurationProvider = new ValueProvider.ValueCollectionProvider_Test().ValueCollection();

            return source;
        }
    }
}
