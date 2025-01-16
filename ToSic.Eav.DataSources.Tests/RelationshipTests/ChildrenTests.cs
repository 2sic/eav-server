using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.DataSources;
using static ToSic.Eav.DataSourceTests.RelationshipTests.RelationshipTestSpecs;

namespace ToSic.Eav.DataSourceTests.RelationshipTests;

[TestClass]
public class ChildrenTests: ChildParentTestBase<Children>
{
    [TestMethod]
    public void PersonsAllWithoutFieldReturnAllCompanies()
    {
        var cl = PrepareDsWithOptions(Person, null, optionsForLastDs: new
        {
            FieldName = Company,
            FilterDuplicates = false
        });
        //cl.FilterDuplicates = false;
        Assert.AreEqual(3, cl.ListForTests().Count());
    }

    [TestMethod]
    public void PersonsOneGetOneCompany()
    {
        var cl = PrepareDs(Person, new []{ PersonWithCompany }, Company);
        Assert.AreEqual(PersonCompanyCount, cl.ListForTests().Count());
    }

    [TestMethod]
    public void CompanyOneHas5Children()
    {
        var cl = PrepareDs(Company, new []{ CompanyIdWithCountryAnd4Categories });
        Assert.AreEqual(5, cl.ListForTests().Count());
    }
    [TestMethod]
    public void CompanyOneHas4Categories()
    {
        var cl = PrepareDs(Company, new []{ CompanyIdWithCountryAnd4Categories }, Categories);
        Assert.AreEqual(4, cl.ListForTests().Count());
    }
    [TestMethod]
    public void CompanyOneHas1Country()
    {
        var cl = PrepareDs(Company, new []{ CompanyIdWithCountryAnd4Categories }, Country);
        Assert.AreEqual(1, cl.ListForTests().Count());
    }

    [TestMethod]
    public void InButNoFieldNameReturnLotsOfChildren() => Assert.IsTrue(PrepareDs().ListForTests().Count() > 12);

    [TestMethod]
    public void InButNoFieldNameReturnLotsOfChildrenFilterDups()
    {
        var unfiltered = PrepareDsWithOptions(optionsForLastDs: new { FilterDuplicates = false });
        var filterDups = PrepareDs();

        Trace.WriteLine("Unfiltered Count: " + unfiltered.ListForTests().Count());
        Trace.WriteLine("Filtered Count: " + filterDups.ListForTests().Count());

        Assert.AreNotEqual(filterDups.ListForTests().Count(), unfiltered.ListForTests().Count());
    }
}