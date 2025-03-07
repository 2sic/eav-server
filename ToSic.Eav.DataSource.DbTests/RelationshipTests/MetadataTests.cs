using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource.DbTests;
using ToSic.Eav.DataSourceTests;
using ToSic.Eav.LookUp;
using ToSic.Eav.Testing;
using ToSic.Testing;
using static ToSic.Eav.RelationshipTests.MetadataTestSpecs;

namespace ToSic.Eav.RelationshipTests;

[Startup(typeof(TestStartupFullWithDb))]
public class MetadataTests(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder): IClassFixture<FullDbFixtureScenarioBasic>
{
    private void TestMetadata(int expected, DataSources.Metadata ds) => Equal(expected, ds.ListTac().Count()); //, $"should have {expected} md items");


    [Fact]
    public void MainOtherTotalMetadata() => TestMetadata(1, PrepareDs(TargetTypeOther));

    [Fact]
    public void MainTargetsTotalMetadata() => TestMetadata(ItemsCount, PrepareDs(TargetTypeMain));

    [Fact]
    public void MainTargetsHelpOnly() => TestMetadata(MainHelp, PrepareDs(TargetTypeMain, typeName: HelpTypeName));

    [Fact]
    public void MainTargetsPriceOnly() => TestMetadata(PricesTotal, PrepareDs(TargetTypeMain, typeName: PriceTypeName));

    [Fact]
    public void MainSingleItem() => TestMetadata(Item2P1HTotal, PrepareDs(TargetTypeMain, ids: [Item2P1H]));

    [Fact]
    public void MainSingleItemPriceOnly() => TestMetadata(PricesTotal, PrepareDs(TargetTypeMain, typeName: PriceTypeName, ids:
        [Item2P1H]));



    protected DataSources.Metadata PrepareDs(string appType = null, IEnumerable<int> ids = null, string typeName = null, ILookUpEngine lookUpEngine = null)
    {
        if(lookUpEngine == null) lookUpEngine = new LookUpTestData(dataBuilder).AppSetAndRes();

        var baseDs = dsSvc.DataSourceSvc.CreateDefault(new DataSourceOptions { AppIdentityOrReader = AppIdentity, LookUp = lookUpEngine } );
        var appDs = dsSvc.CreateDataSource<App>(baseDs);

        var inStream = dsSvc.FilterStreamByIds(ids, appDs.GetStream(appType));

        var dsParams = new Dictionary<string, object>();
        if (typeName != null)
            dsParams["ContentTypeName"] = typeName;

        var childDs = dsSvc.CreateDataSource<DataSources.Metadata>(inStream, dsParams);
        //if (typeName != null)
        //    childDs.ContentTypeName = typeName;

        return childDs;
    }

}