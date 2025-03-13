using ToSic.Eav.Persistence.File;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests19.Persistence.File;


public class FileLoaderContentTypes(ITestOutputHelper output, FileSystemLoader loaderRaw) : ServiceBase("test"), IClassFixture<DoFixtureStartup<ScenarioMini>>
{

    [Fact]
    public void FLoader_LoadAllAndCount()
    {
        var expected = 8;
        var cts = new LoaderHelper(PersistenceTestConstants.ScenarioMiniDeep, Log).LoadAllTypes(loaderRaw, Log);
        Equal(8, cts.Count);//, $"test case has {expected} content-types to deserialize, found {cts.Count}");
    }

    [Fact]
    public void FLoader_SqlType()
    {
        var cts = new LoaderHelper(PersistenceTestConstants.ScenarioMiniDeep, Log).LoadAllTypes(loaderRaw, Log);
        var sqlType = cts.FirstOrDefault(ct => ct.NameId.Contains("Sql"));
        NotNull(sqlType);//, "should find the sql type");
        Equal("System", sqlType.Scope);// "scope should be system");
    }

    [Fact]
    public void FLoader_CheckDynamicAttributes()
    {
        var cts = new LoaderHelper(PersistenceTestConstants.ScenarioMiniDeep, Log).LoadAllTypes(loaderRaw, Log);
        var sqlType = cts.First(ct => ct.NameId.Contains("Sql"));
        Equal(9, sqlType.Attributes.Count());//, "sql type should have x attributes");

        var conStrName = "ConnectionString";
        var conStr = sqlType.Attributes.FirstOrDefault(a => a.Name == conStrName);
        NotNull(conStr);//, $"should find the {conStrName} field");

        var title = sqlType.Attributes.FirstOrDefault(a => a.IsTitle);
        NotNull(title);//, "should find title field");

        var conMeta = conStr.Metadata;
        Equal(2, conMeta.Count());//, "constr should have 2 meta-items");

        var conMetaAll = conMeta.FirstOrDefault(e => e.Type.Name == AttributeMetadata.TypeGeneral);
        NotNull(conMetaAll);//, "should have @all metadata");

        var conMetaStr = conMeta.FirstOrDefault(e => e.Type.Name == "@string-default");
        NotNull(conMetaStr);//, "should have string metadata");

        var lines = (decimal)conMetaStr.GetTac("RowCount");
        Equal(3, lines);
    }



    [Fact]
    public void FLoader_CheckTypeMetadata()
    {
        var cts = new LoaderHelper(PersistenceTestConstants.ScenarioMiniDeep, Log).LoadAllTypes(loaderRaw, Log);
        var sqlType = cts.First(ct => ct.NameId.Contains("Sql"));
        Equal(9, sqlType.Attributes.Count());//, "sql type should have x attributes");


        var meta = sqlType.Metadata;
        Equal(2, meta.Count());//, "should have 2 meta-items");

        var conMetaAll = meta.FirstOrDefault(e => e.Type.Name == "Basics");
        NotNull(conMetaAll);//, "should have Basics metadata");

        var niceName = conMetaAll.GetTac<string>("NiceName");
        Equal("sql content type", niceName);
        var active = conMetaAll.GetTac<bool>("Active");
        True(active);//, "should have an active-info");


        var conMetaStr = meta.FirstOrDefault(e => e.Type.Name == "Enhancements");
        NotNull(conMetaStr);//, "should have Enhancements metadata");

        var icon = conMetaStr.GetTac<string>("Icon");
        Equal("icon.png", icon);
    }


}