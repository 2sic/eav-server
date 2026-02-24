using ToSic.Eav.DataSource.Sys.Catalog;
using ToSic.Eav.DataSource.VisualQuery.Sys;

namespace ToSic.Eav.DataSource.Catalog;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class DataSourceCatalogTests(DataSourceCatalog dsCatalog)
{
    // Note that these numbers will change as we refactor everything
    // since some data sources will be in a later project and not loaded at this time
    // So adjust as you refactor it.
    public const int EavInstalledDsCount = 49; // with 2sxc v21 it's ca. 49;
    public const int TestingAddedDsCount = 2;
    public const int StandardInstalledDSCount = EavInstalledDsCount + TestingAddedDsCount;

    public const int StandardInstalledPipeLineDS = 42; // with 2sxc v21 it's 42

    /// <summary>
    /// Since we're counting data sources, and the number changes from time to time, we want to allow some flexibility.
    /// </summary>
    private const int TestFlexibility = 3;
    public const string SqlFullName = "ToSic.Eav.DataSources.Sql";
    public const string DeferredFullName = "ToSic.Eav.DataSources.DeferredPipelineQuery";

    [Theory]
    [InlineData(true, StandardInstalledPipeLineDS)]
    [InlineData(false, StandardInstalledDSCount)]
    public void CountDataSources(bool onlyForVisualQuery, int expected)
    {
        var dsList = dsCatalog.GetAll(onlyForVisualQuery, 0);
        InRange(dsList.Count, expected - TestFlexibility, expected + TestFlexibility);
    }

    [Theory]
    [InlineData(SqlFullName, false)]
    [InlineData(DeferredFullName, true)]
    public void TryFindDataSourceByName(string name, bool expectedNull)
    {
        var dsList = dsCatalog.GetAll(false, 0);
        var sqlDs = dsList.FirstOrDefault(c => c.Type.FullName == name);
        if (expectedNull)
            Null(sqlDs);
        else
            NotNull(sqlDs);
    }

    [Fact]
    public void NoNameIdsAreDuplicate()
    {
        var dsList = dsCatalog.GetAll(false, 0);
        var grouped = dsList
            .GroupBy(c => c.NameId)
            // Skip the ones without a key
            .Where(g => g.Key != DataSourceInfo.ErrorNoNameId)
            .ToList();

        foreach (var group in grouped)
            Single(group);

        var withoutId = dsList.Where(c => c.NameId == DataSourceInfo.ErrorNoNameId);
        Equal(dsList.Count, grouped.Count + withoutId.Count());
    }

}