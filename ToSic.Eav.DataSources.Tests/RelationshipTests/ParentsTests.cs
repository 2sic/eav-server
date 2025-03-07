using static ToSic.Eav.DataSourceTests.RelationshipTests.RelationshipTestSpecs;

namespace ToSic.Eav.DataSourceTests.RelationshipTests;

[TestClass]
public class ParentsTests: ChildParentTestBase<Parents>
{

    [TestMethod]
    public void PersonsOneHasNoParents()
    {
        var cl = PrepareDs(RelationshipTestSpecs.Person, [PersonWithCompany], Company);
        AreEqual(0, cl.ListTac().Count());
    }

    [TestMethod]
    public void CountrySwitzerlandHas2Parents()
    {
        var cl = PrepareDs(Country, [CountrySwitzerland]);
        AreEqual(CountrySwitzerlandParents, cl.ListTac().Count());
    }
        
}