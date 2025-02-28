using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSources;
using ToSic.Testing.Shared;
using static ToSic.Eav.DataSourceTests.RelationshipTests.MetadataTestSpecs;

namespace ToSic.Eav.DataSourceTests.RelationshipTests;

[TestClass]
public class MetadataTargetsTests : TestBaseDiEavFullAndDb
{
    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

    private void TestMetadataTargets(int expected, MetadataTargets ds) 
        => Assert.AreEqual(expected, ds.ListForTests().Count(), $"should have {expected} md items");

    [TestMethod]
    public void AllMetadataOfHelp() => TestMetadataTargets(TotalHelp, PrepareDs(HelpTypeName));

    [TestMethod]
    public void MainMetadataOfHelp() => TestMetadataTargets(MainHelp, PrepareDs(HelpTypeName, typeName: TargetTypeMain));

    [TestMethod]
    public void TargetOfHelp1() => TestMetadataTargets(1, PrepareDs(ids: [MetaHelpOn1]));

    [TestMethod]
    public void TargetOfHelp2() => TestMetadataTargets(1, PrepareDs(ids: [MetaHelpOn2]));


    [TestMethod]
    public void PriceTargetsDefaultOfAllTypes() => TestMetadataTargets(PriceTargetsUnique, PrepareDs(PriceTypeName));

    [TestMethod]
    public void PriceTargetsDefault() => TestMetadataTargets(PriceTargetsUnique, PrepareDs(PriceTypeName, typeName: TargetTypeMain));

    [TestMethod]
    public void PriceTargetsFilterDups() => TestMetadataTargets(PriceTargetsUnique, PrepareDs(PriceTypeName, typeName: TargetTypeMain, deduplicate: true));

    [TestMethod]
    public void PriceTargetsDontFilterDups() => TestMetadataTargets(PriceTargetsWithDups, PrepareDs(PriceTypeName, typeName: TargetTypeMain, deduplicate: false));


    protected MetadataTargets PrepareDs(string appType = null, IEnumerable<int> ids = null, string typeName = null, bool? deduplicate = null)
    {
        var lookUpEngine = new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes();

        var baseDs = DsSvc.DataSourceSvc.CreateDefault(new DataSourceOptions { AppIdentityOrReader = AppIdentity, LookUp = lookUpEngine });
        var appDs = DsSvc.CreateDataSource<App>(baseDs);

        var inStream = FilterStreamByIds(ids, appDs.GetStream(appType));

        var dsParams = new Dictionary<string, object>();
        if (typeName != null)
            dsParams["ContentTypeName"] = typeName;

        if(deduplicate != null)
            dsParams["FilterDuplicates"] = deduplicate.Value;

        var childDs = DsSvc.CreateDataSource<MetadataTargets>(inStream, dsParams);
        //if (typeName != null)
        //    childDs.ContentTypeName = typeName;

        //if(deduplicate != null)
        //    childDs.FilterDuplicates = deduplicate.Value;

        // todo: unique

        return childDs;
    }

}