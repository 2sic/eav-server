using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource.DbTests;
using ToSic.Eav.DataSourceTests;
using static ToSic.Eav.RelationshipTests.RelationshipTestSpecs;

namespace ToSic.Eav.RelationshipTests;

[Startup(typeof(TestStartupFullWithDb))]
public class ParentsTests(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder) : ChildParentTestBase<Parents>(dsSvc, dataBuilder), IClassFixture<FullDbFixture>
{

    [Fact]
    public void PersonsOneHasNoParents()
    {
        var cl = PrepareDs(RelationshipTestSpecs.Person, [PersonWithCompany], Company);
        Empty(cl.ListTac());
    }

    [Fact]
    public void CountrySwitzerlandHas2Parents()
    {
        var cl = PrepareDs(Country, [CountrySwitzerland]);
        Equal(CountrySwitzerlandParents, cl.ListTac().Count());
    }
        
}