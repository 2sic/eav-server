using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSourceTests;
using ToSic.Eav.LookUp;
using ToSic.Eav.Testing;

namespace ToSic.Eav.DataSource.DbTests.DataSource.Sql;

[Startup(typeof(StartupTestFullWithDb))]
public class SqlDsTst(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder): IClassFixture<FullDbFixtureScenarioBasic>
{
    private const string ConnectionDummy = "";
    private const string ContentTypeName = "SqlData";

    [Fact]
    public void SqlDataSource_NoConfigChangesIfNotNecessary()
    {
        var initQuery = "Select * From Products";
        var sql = GenerateSqlDataSource(ConnectionDummy, initQuery, ContentTypeName);
        var config = sql.Configuration.Values;
        var configCountBefore = config.Count;
        sql.Configuration.Parse(); 

        Equal(configCountBefore, config.Count);
        Equal(initQuery, sql.SelectCommand);
    }

    #region test parameter injection
    [Fact]
    public void SqlDataSource_SqlInjectionProtection()
    {
        var initQuery = "Select * From Products Where ProductId = [QueryString:Id]";
        var expectedQuery = "Select * From Products Where ProductId = @" + DataSources.Sql.ExtractedParamPrefix + "1";
        var paramsInQuery = 1;
        var sql = GenerateSqlDataSource(ConnectionDummy, initQuery, ContentTypeName);
        var config = sql.Configuration.Values;

        var configCountBefore = config.Count;
        sql.CustomConfigurationParse();

        Equal(configCountBefore + paramsInQuery, config.Count);
        Equal(expectedQuery, sql.SelectCommand);

        sql.Configuration.Parse();
        var parsed = sql.Configuration.Values;
        Equal("", parsed["@" + DataSources.Sql.ExtractedParamPrefix + "1"]);
    }

    [Fact]
    public void SqlDataSource_SqlInjectionProtectionComplex()
    {
        var initQuery = @"Select Top [AppSettings:MaxPictures] * 
From Products 
Where CatName = [AppSettings:DefaultCategoryName||NotFound] 
And ProductSort = [AppSettings:DoesntExist||CorrectlyDefaulted]";
        var expectedQuery = @"Select Top @" + DataSources.Sql.ExtractedParamPrefix + @"1 * 
From Products 
Where CatName = @" + DataSources.Sql.ExtractedParamPrefix + @"2 
And ProductSort = @" + DataSources.Sql.ExtractedParamPrefix + @"3";
        var paramsInQuery = 3;

        var sql = GenerateSqlDataSource(ConnectionDummy, initQuery, ContentTypeName);
        var config = sql.Configuration.Values;
        var configCountBefore = config.Count;
        sql.CustomConfigurationParse();

        Equal(configCountBefore + paramsInQuery, config.Count);
        Equal(expectedQuery, sql.SelectCommand);

        sql.Configuration.Parse();
        var parsed = sql.Configuration.Values;
        Equal(LookUpTestConstants.MaxPictures, parsed["@" + DataSources.Sql.ExtractedParamPrefix + "1"]);
        Equal(LookUpTestConstants.DefaultCategory, parsed["@" + DataSources.Sql.ExtractedParamPrefix + "2"]);
        Equal("CorrectlyDefaulted", parsed["@" + DataSources.Sql.ExtractedParamPrefix + "3"]);
    }

    #endregion

    #region test bad sql statements like insert / drop etc.
    [Fact]
    public void SqlDataSource_BadSqlInsert()
    {
        var results = GenerateSqlDataSource(ConnectionDummy, "Insert something into something", ContentTypeName);
        DataSourceErrors.VerifyStreamIsError(results, DataSources.Sql.ErrorTitleForbiddenSql);
    }


    [Fact]
    public void SqlDataSource_BadSqlSelectInsert()
        => DataSourceErrors.VerifyStreamIsError(
            GenerateSqlDataSource(ConnectionDummy, "Select * from table; Insert something", ContentTypeName),
            DataSources.Sql.ErrorTitleForbiddenSql
        );

    [Fact]
    public void SqlDataSource_BadSqlSpaceInsert()
        => DataSourceErrors.VerifyStreamIsError(
            GenerateSqlDataSource(ConnectionDummy, " Insert something into something", ContentTypeName),
            DataSources.Sql.ErrorTitleForbiddenSql);

    [Fact]
    public void SqlDataSource_BadSqlDropInsert()
        => DataSourceErrors.VerifyStreamIsError(
            GenerateSqlDataSource(ConnectionDummy, "Drop tablename", ContentTypeName),
            DataSources.Sql.ErrorTitleForbiddenSql);

    [Fact]
    public void SqlDataSource_BadSqlSelectDrop()
        => DataSourceErrors.VerifyStreamIsError(
            GenerateSqlDataSource(ConnectionDummy, "Select * from Products; Drop tablename", ContentTypeName),
            DataSources.Sql.ErrorTitleForbiddenSql);

    #endregion


    #region test title / entityid fields with casing

    [Fact]
    public void SqlDataSource_TitleCasing()
    {
        var select = "SELECT [PortalID] as entityId, HomeDirectory As entityTitle, " +
                     "[AdministratorId],[GUID],[HomeDirectory],[PortalGroupID] " +
                     "FROM [Portals]";
        var sql = GenerateSqlDataSource(EavTestConfig.ScenarioBasic.ConStr, select, ContentTypeName);
        var list = sql.ListTac();
        True(list.Any(), "found some");
    }

    #endregion

    public DataSources.Sql GenerateSqlDataSource(string connection, string query, string typeName)
    {
        return dsSvc.CreateDataSource<DataSources.Sql>(new LookUpTestData(dataBuilder).AppSetAndRes())
            .Setup(connection, query, typeName);
    }
}