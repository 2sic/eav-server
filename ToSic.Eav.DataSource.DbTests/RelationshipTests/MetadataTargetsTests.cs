using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSourceTests;
using ToSic.Eav.LookUp;
using static ToSic.Eav.DataSource.DbTests.RelationshipTests.MetadataTestSpecs;

namespace ToSic.Eav.DataSource.DbTests.RelationshipTests;

[Startup(typeof(StartupTestFullWithDb))]
public class MetadataTargetsTests(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder): IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    private void TestMetadataTargets(int expected, MetadataTargets ds) 
        => Equal(expected, ds.ListTac().Count());//, $"should have {expected} md items");

    [Fact] public void AllMetadataOfHelp() =>
        TestMetadataTargets(TotalHelp, PrepareDs(HelpTypeName));

    [Fact] public void MainMetadataOfHelp() => 
        TestMetadataTargets(MainHelp, PrepareDs(HelpTypeName, typeName: TargetTypeMain));

    [Fact] public void TargetOfHelp1() =>
        TestMetadataTargets(1, PrepareDs(ids: [MetaHelpOn1]));

    [Fact] public void TargetOfHelp2() =>
        TestMetadataTargets(1, PrepareDs(ids: [MetaHelpOn2]));


    [Fact] public void PriceTargetsDefaultOfAllTypes() =>
        TestMetadataTargets(PriceTargetsUnique, PrepareDs(PriceTypeName));

    [Fact] public void PriceTargetsDefault() =>
        TestMetadataTargets(PriceTargetsUnique, PrepareDs(PriceTypeName, typeName: TargetTypeMain));

    [Fact] public void PriceTargetsFilterDups() =>
        TestMetadataTargets(PriceTargetsUnique, PrepareDs(PriceTypeName, typeName: TargetTypeMain, deduplicate: true));

    [Fact] public void PriceTargetsDontFilterDups() =>
        TestMetadataTargets(PriceTargetsWithDups, PrepareDs(PriceTypeName, typeName: TargetTypeMain, deduplicate: false));


    protected MetadataTargets PrepareDs(string appType = null, IEnumerable<int> ids = null, string typeName = null, bool? deduplicate = null)
    {
        var lookUpEngine = new LookUpTestData(dataBuilder).AppSetAndRes();

        var baseDs = dsSvc.DataSourceSvc.CreateDefault(new DataSourceOptions { AppIdentityOrReader = AppIdentity, LookUp = lookUpEngine });
        var appDs = dsSvc.CreateDataSource<App>(baseDs);

        var inStream = dsSvc.FilterStreamByIds(ids, appDs.GetStream(appType));

        var dsParams = new Dictionary<string, object>();
        if (typeName != null)
            dsParams["ContentTypeName"] = typeName;

        if(deduplicate != null)
            dsParams["FilterDuplicates"] = deduplicate.Value;

        var childDs = dsSvc.CreateDataSource<MetadataTargets>(inStream, dsParams);
        //if (typeName != null)
        //    childDs.ContentTypeName = typeName;

        //if(deduplicate != null)
        //    childDs.FilterDuplicates = deduplicate.Value;

        // todo: unique

        return childDs;
    }

}