using System.Diagnostics;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSourceTests;
using ToSic.Eav.Testing;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSource.DbTests.RelationshipTests;

[Startup(typeof(StartupTestFullWithDb))]
public partial class RelationshipFilterTest(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder) : RelationshipTestBase(dsSvc, dataBuilder), IClassFixture<FullDbFixtureScenarioBasic>
{

    // todo: necessary tests
    // title compare on string
    // title compare on int
    // title compare on bool?
    // title compare on relationship-title
    // non-title compare on string
    // non-title compare on int
    // parent filtering on child
    // child filtering on parent
    // having-children
    // having-exactly x-children
    // ...all compare modes: contains, first, none,...
        

    [Fact]
    public void DS_RelFil_ConstructionWorks()
    {
        var relFilt = BuildRelationshipFilter(RelationshipTestSpecs.Company);
        Trace.Write(Log.Dump());
        NotNull(relFilt);//, "relFilt != null");
    }

    [Fact]
    public void DS_RelFil_NoConfigEmpty()
    {
        var relFilt = BuildRelationshipFilter(RelationshipTestSpecs.Company);

        var result = relFilt.ListTac().ToList();

        Trace.Write(Log.Dump());

        True(result.Count == 0, "result.Count == 0");
    }
    [Fact]
    public void DS_RelFil_NoConfigFallback()
    {
        var relFilt = BuildRelationshipFilter(RelationshipTestSpecs.Company);
        relFilt.AttachTac(DataSourceConstants.StreamFallbackName, relFilt.InTac()[DataSourceConstants.StreamDefaultName]);

        var result = relFilt.ListTac().ToList();

        Trace.Write(Log.Dump());
        True(result.Count > 0, "count should be more than 0, as it should use fallback");
    }

    [Fact]
    public void DS_RelFil_Companies_Having_Category_Title() 
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title", RelationshipTestSpecs.Company, CompCat, CatWeb)
            .Run(true);

    [Fact]
    public void DS_RelFil_Companies_Having_Category_Active() 
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title", RelationshipTestSpecs.Company, CompCat, "true", relAttribute: "Active")
            .Run(true);

    [Fact]
    public void DS_RelFil_Companies_Having_InexistingProperty_Title() 
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-inexisting-property", RelationshipTestSpecs.Company, CompInexistingProp, CatWeb)
            .Run(false);

}