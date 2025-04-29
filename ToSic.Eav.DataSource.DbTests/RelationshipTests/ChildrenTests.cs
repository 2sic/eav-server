using System.Diagnostics;
using ToSic.Eav.Data.Build;
using static ToSic.Eav.DataSource.DbTests.RelationshipTests.RelationshipTestSpecs;

namespace ToSic.Eav.DataSource.DbTests.RelationshipTests;

[Startup(typeof(StartupTestFullWithDb))]
public class ChildrenTests(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder) : ChildParentTestBase<Children>(dsSvc, dataBuilder), IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    [Fact]
    public void PersonsAllWithoutFieldReturnAllCompanies()
    {
        var cl = PrepareDsWithOptions(RelationshipTestSpecs.Person, null, optionsForLastDs: new
        {
            FieldName = Company,
            FilterDuplicates = false
        });
        //cl.FilterDuplicates = false;
        Equal(3, cl.ListTac().Count());
    }

    [Fact]
    public void PersonsOneGetOneCompany()
    {
        var cl = PrepareDs(RelationshipTestSpecs.Person, [PersonWithCompany], Company);
        Equal(PersonCompanyCount, cl.ListTac().Count());
    }

    [Fact]
    public void CompanyOneHas5Children()
    {
        var cl = PrepareDs(Company, [CompanyIdWithCountryAnd4Categories]);
        Equal(5, cl.ListTac().Count());
    }
    [Fact]
    public void CompanyOneHas4Categories()
    {
        var cl = PrepareDs(Company, [CompanyIdWithCountryAnd4Categories], Categories);
        Equal(4, cl.ListTac().Count());
    }
    [Fact]
    public void CompanyOneHas1Country()
    {
        var cl = PrepareDs(Company, [CompanyIdWithCountryAnd4Categories], Country);
        Single(cl.ListTac());
    }

    [Fact]
    public void InButNoFieldNameReturnLotsOfChildren() => True(PrepareDs().ListTac().Count() > 12);

    [Fact]
    public void InButNoFieldNameReturnLotsOfChildrenFilterDups()
    {
        var unfiltered = PrepareDsWithOptions(optionsForLastDs: new { FilterDuplicates = false });
        var filterDups = PrepareDs();

        Trace.WriteLine("Unfiltered Count: " + unfiltered.ListTac().Count());
        Trace.WriteLine("Filtered Count: " + filterDups.ListTac().Count());

        NotEqual(filterDups.ListTac().Count(), unfiltered.ListTac().Count());
    }
}