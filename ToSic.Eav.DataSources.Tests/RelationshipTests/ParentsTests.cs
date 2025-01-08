using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.DataSources;
using static ToSic.Eav.DataSourceTests.RelationshipTests.RelationshipTestSpecs;

namespace ToSic.Eav.DataSourceTests.RelationshipTests;

[TestClass]
public class ParentsTests: ChildParentTestBase<Parents>
{

    [TestMethod]
    public void PersonsOneHasNoParents()
    {
        var cl = PrepareDs(Person, new []{ PersonWithCompany }, Company);
        Assert.AreEqual(0, cl.ListForTests().Count());
    }

    [TestMethod]
    public void CountrySwitzerlandHas2Parents()
    {
        var cl = PrepareDs(Country, new []{ CountrySwitzerland });
        Assert.AreEqual(CountrySwitzerlandParents, cl.ListForTests().Count());
    }
        
}