﻿using ToSic.Eav.Data.Build;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSourceTests.ExternalData;

[TestClass]
public class SqlDsTst: TestBaseEavDataSource
{
    private const string ConnectionDummy = "";
    private const string ContentTypeName = "SqlData";

    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

    [TestMethod]
    public void SqlDataSource_NoConfigChangesIfNotNecessary()
    {
        var initQuery = "Select * From Products";
        var sql = GenerateSqlDataSource(ConnectionDummy, initQuery, ContentTypeName);
        var config = sql.Configuration.Values;
        var configCountBefore = config.Count;
        sql.Configuration.Parse(); 

        AreEqual(configCountBefore, config.Count);
        AreEqual(initQuery, sql.SelectCommand);
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

        AreEqual(configCountBefore + paramsInQuery, config.Count);
        AreEqual(expectedQuery, sql.SelectCommand);

        sql.Configuration.Parse();
        var parsed = sql.Configuration.Values;
        AreEqual("", parsed["@" + Sql.ExtractedParamPrefix + "1"]);
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

        AreEqual(configCountBefore + paramsInQuery, config.Count);
        AreEqual(expectedQuery, sql.SelectCommand);

        sql.Configuration.Parse();
        var parsed = sql.Configuration.Values;
        AreEqual(LookUpTestConstants.MaxPictures, parsed["@" + Sql.ExtractedParamPrefix + "1"]);
        AreEqual(LookUpTestConstants.DefaultCategory, parsed["@" + Sql.ExtractedParamPrefix + "2"]);
        AreEqual("CorrectlyDefaulted", parsed["@" + Sql.ExtractedParamPrefix + "3"]);
    }

    #endregion

    #region test bad sql statements like insert / drop etc.
    [TestMethod]
    public void SqlDataSource_BadSqlInsert()
    {
        var results = GenerateSqlDataSource(ConnectionDummy, "Insert something into something", ContentTypeName);
        DataSourceErrors.VerifyStreamIsError(results, Sql.ErrorTitleForbiddenSql);
    }


    [TestMethod]
    public void SqlDataSource_BadSqlSelectInsert()
        => DataSourceErrors.VerifyStreamIsError(
            GenerateSqlDataSource(ConnectionDummy, "Select * from table; Insert something", ContentTypeName),
            Sql.ErrorTitleForbiddenSql
        );

    [TestMethod]
    public void SqlDataSource_BadSqlSpaceInsert()
        => DataSourceErrors.VerifyStreamIsError(
            GenerateSqlDataSource(ConnectionDummy, " Insert something into something", ContentTypeName),
            Sql.ErrorTitleForbiddenSql);

    [TestMethod]
    public void SqlDataSource_BadSqlDropInsert()
        => DataSourceErrors.VerifyStreamIsError(
            GenerateSqlDataSource(ConnectionDummy, "Drop tablename", ContentTypeName),
            Sql.ErrorTitleForbiddenSql);

    [TestMethod]
    public void SqlDataSource_BadSqlSelectDrop()
        => DataSourceErrors.VerifyStreamIsError(
            GenerateSqlDataSource(ConnectionDummy, "Select * from Products; Drop tablename", ContentTypeName),
            Sql.ErrorTitleForbiddenSql);

    #endregion


    #region test title / entityid fields with casing

    [TestMethod]
    public void SqlDataSource_TitleCasing()
    {
        var select = "SELECT [PortalID] as entityId, HomeDirectory As entityTitle, " +
                     "[AdministratorId],[GUID],[HomeDirectory],[PortalGroupID] " +
                     "FROM [Portals]";
        var sql = GenerateSqlDataSource(TestConfig.ConStr, select, ContentTypeName);
        var list = sql.ListTac();
        IsTrue(list.Any(), "found some");
    }

    #endregion

    public Sql GenerateSqlDataSource(string connection, string query, string typeName)
    {
        return DsSvc.CreateDataSource<Sql>(new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes())
            .Setup(connection, query, typeName);
    }
}