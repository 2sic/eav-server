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
        Assert.AreEqual(3, cl.ListTac().Count());
    }

    [TestMethod]
    public void PersonsOneGetOneCompany()
    {
        var cl = PrepareDs(Person, [PersonWithCompany], Company);
        Assert.AreEqual(PersonCompanyCount, cl.ListTac().Count());
    }

    [TestMethod]
    public void CompanyOneHas5Children()
    {
        var cl = PrepareDs(Company, [CompanyIdWithCountryAnd4Categories]);
        Assert.AreEqual(5, cl.ListTac().Count());
    }
    [TestMethod]
    public void CompanyOneHas4Categories()
    {
        var cl = PrepareDs(Company, [CompanyIdWithCountryAnd4Categories], Categories);
        Assert.AreEqual(4, cl.ListTac().Count());
    }
    [TestMethod]
    public void CompanyOneHas1Country()
    {
        var cl = PrepareDs(Company, [CompanyIdWithCountryAnd4Categories], Country);
        Assert.AreEqual(1, cl.ListTac().Count());
    }

    [TestMethod]
    public void InButNoFieldNameReturnLotsOfChildren() => Assert.IsTrue(PrepareDs().ListTac().Count() > 12);

    [TestMethod]
    public void InButNoFieldNameReturnLotsOfChildrenFilterDups()
    {
        var unfiltered = PrepareDsWithOptions(optionsForLastDs: new { FilterDuplicates = false });
        var filterDups = PrepareDs();

        Trace.WriteLine("Unfiltered Count: " + unfiltered.ListTac().Count());
        Trace.WriteLine("Filtered Count: " + filterDups.ListTac().Count());

        Assert.AreNotEqual(filterDups.ListTac().Count(), unfiltered.ListTac().Count());
    }
}