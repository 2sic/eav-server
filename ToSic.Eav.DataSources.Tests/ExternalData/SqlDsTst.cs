﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.DataSources;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.ExternalData
{
    [TestClass]
    public class SqlDsTst
    {
        private const string ConnectionDummy = "";
        private const string ConnectionName = TestConstants.ConStr;// "Data Source=.\\SQLExpress;Initial Catalog=2flex 2Sexy Content;Integrated Security=True";
        private const string ContentTypeName = "SqlData";

        [TestMethod]
        public void SqlDataSource_NoConfigChangesIfNotNecessary()
        {
            var initQuery = "Select * From Products";
            var sql = GenerateSqlDataSource(ConnectionDummy, initQuery, ContentTypeName);
            var config = sql.Configuration.Values;
            var configCountBefore = config.Count;
            sql.Configuration.Parse(); 

            Assert.AreEqual(configCountBefore, config.Count);
            Assert.AreEqual(initQuery, sql.SelectCommand);
        }

        #region test parameter injection
        [TestMethod]
        public void SqlDataSource_SqlInjectionProtection()
        {
            var initQuery = "Select * From Products Where ProductId = [QueryString:Id]";
            var expectedQuery = "Select * From Products Where ProductId = @" + Sql.ExtractedParamPrefix + "1";
            var paramsInQuery = 1;
            var sql = GenerateSqlDataSource(ConnectionDummy, initQuery, ContentTypeName);
            var config = sql.Configuration.Values;

            var configCountBefore = config.Count;
            sql.CustomConfigurationParse();

            Assert.AreEqual(configCountBefore + paramsInQuery, config.Count);
            Assert.AreEqual(expectedQuery, sql.SelectCommand);

            sql.Configuration.Parse();
            var parsed = sql.Configuration.Values;
            Assert.AreEqual("", parsed["@" + Sql.ExtractedParamPrefix + "1"]);
        }

        [TestMethod]
        public void SqlDataSource_SqlInjectionProtectionComplex()
        {
            var initQuery = @"Select Top [AppSettings:MaxPictures] * 
From Products 
Where CatName = [AppSettings:DefaultCategoryName||NotFound] 
And ProductSort = [AppSettings:DoesntExist||CorrectlyDefaulted]";
            var expectedQuery = @"Select Top @" + Sql.ExtractedParamPrefix + @"1 * 
From Products 
Where CatName = @" + Sql.ExtractedParamPrefix + @"2 
And ProductSort = @" + Sql.ExtractedParamPrefix + @"3";
            var paramsInQuery = 3;

            var sql = GenerateSqlDataSource(ConnectionDummy, initQuery, ContentTypeName);
            var config = sql.Configuration.Values;
            var configCountBefore = config.Count;
            sql.CustomConfigurationParse();

            Assert.AreEqual(configCountBefore + paramsInQuery, config.Count);
            Assert.AreEqual(expectedQuery, sql.SelectCommand);

            sql.Configuration.Parse();
            var parsed = sql.Configuration.Values;
            Assert.AreEqual(LookUpEngineTests.MaxPictures, parsed["@" + Sql.ExtractedParamPrefix + "1"]);
            Assert.AreEqual(LookUpEngineTests.DefaultCategory, parsed["@" + Sql.ExtractedParamPrefix + "2"]);
            Assert.AreEqual("CorrectlyDefaulted", parsed["@" + Sql.ExtractedParamPrefix + "3"]);
        }

        #endregion

        #region test bad sql statements like insert / drop etc.
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlDataSource_BadSqlInsert() 
            => GenerateSqlDataSource(ConnectionDummy, "Insert something into something", ContentTypeName).Immutable.Any();

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlDataSource_BadSqlSelectInsert() 
            => GenerateSqlDataSource(ConnectionDummy, "Select * from table; Insert something", ContentTypeName).Immutable.Any();

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlDataSource_BadSqlSpaceInsert() 
            => GenerateSqlDataSource(ConnectionDummy, " Insert something into something", ContentTypeName).Immutable.Any();

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlDataSource_BadSqlDropInsert() 
            => GenerateSqlDataSource(ConnectionDummy, "Drop tablename", ContentTypeName).Immutable.Any();

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlDataSource_BadSqlSelectDrop() 
            => GenerateSqlDataSource(ConnectionDummy, "Select * from Products; Drop tablename", ContentTypeName).Immutable.Any();

        #endregion


        #region test title / entityid fields with casing

        [TestMethod]
        public void SqlDataSource_TitleCasing()
        {
            var select = "SELECT [PortalID] as entityId, HomeDirectory As entityTitle, " +
                         "[AdministratorId],[GUID],[HomeDirectory],[PortalGroupID] " +
                         "FROM [Portals]";
            var sql = GenerateSqlDataSource(ConnectionName, select, ContentTypeName);
            var list = sql.Immutable;
            Assert.IsTrue(list.Any(), "found some");
        }

        #endregion

        public static Sql GenerateSqlDataSource(string connection, string query, string typeName)
        {
            var source = new Sql(connection, query, typeName);
            source.Init(LookUpTestData.AppSetAndRes());
            //source.ConfigurationProvider = DemoConfigs.AppSetAndRes();

            return source;
        }
    }
}
