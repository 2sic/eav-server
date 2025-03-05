using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSources;
using ToSic.Testing.Shared;
using ToSic.Testing.Shared.Data;
using static ToSic.Eav.DataSourceTests.TestData.PersonSpecs;

namespace ToSic.Eav.DataSourceTests;
// Todo
// 1. Tests with standard fields like EntityTitle
// 2. Tests with multiple fileds like City,Name
// Later: Create tests with language-parameters as well, as these tests ignore the language and always use default

[TestClass]
public class ValueSort_String: TestBaseEavDataSource
{
    private const int TestVolume = 30;
    private readonly ValueSort _testDataGeneratedOutsideTimer;
    private readonly ValueFilterMaker _valueFilterMaker;

    public ValueSort_String()
    {
        _valueFilterMaker = new ValueFilterMaker(this);

        _testDataGeneratedOutsideTimer =
            _valueFilterMaker.GeneratePersonSourceWithDemoData(TestVolume);
    }

    [TestMethod]
    public void ValueSort_Unused()
    {
        var vf = _testDataGeneratedOutsideTimer;
        var listOut = vf.ListTac().ToList();
        var listIn = vf.InTac()[DataSourceConstants.StreamDefaultName].ListTac().ToList();
        CollectionAssert.AreEqual(listOut, listIn, "Lists should be the same if no criteria applied");
    }

    [TestMethod]
    public void ValueSort_SortFieldOnly_City()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attributes = FieldCity;
        var result = vf.ListTac().ToList();
        // check that each following city is same or larger...
        ValidateFieldIsSorted(result, FieldCity, true);
    }

    // Initial control test using table, not entities which could be ML
    [DataRow(FieldCity, UseDataTable, false, "asc", true, "")]

    // Basic tests single language
    [DataRow(FieldCity, UseEntitiesL, false, "",     true, "No direction should = asc.")]
    [DataRow(FieldCity, UseEntitiesL, false, "a",    true, "Sort city")]
    [DataRow(FieldCity, UseEntitiesL, false, "1",    true, "sort city")]
    [DataRow(FieldCity, UseEntitiesL, false, "<",    true, "")]
    [DataRow(FieldCity, UseEntitiesL, false, "desc", false, "")]
    [DataRow(FieldCity, UseEntitiesL, false, "d",    false, "")]
    [DataRow(FieldCity, UseEntitiesL, false, "D",    false, "")]
    [DataRow(FieldCity, UseEntitiesL, false, "0",    false, "")]
    [DataRow(FieldCity, UseEntitiesL, false, ">",    false, "")]

    // same tests now ML, results should be the same
    [DataRow(FieldCity, UseEntitiesL, true, "",     true, "No direction should = asc.")]
    [DataRow(FieldCity, UseEntitiesL, true, "a",    true, "Sort city")]
    [DataRow(FieldCity, UseEntitiesL, true, "1",    true, "sort city")]
    [DataRow(FieldCity, UseEntitiesL, true, "<",    true, "")]
    [DataRow(FieldCity, UseEntitiesL, true, "desc", false, "")]
    [DataRow(FieldCity, UseEntitiesL, true, "d",    false, "")]
    [DataRow(FieldCity, UseEntitiesL, true, "D",    false, "")]
    [DataRow(FieldCity, UseEntitiesL, true, "0",    false, "")]
    [DataRow(FieldCity, UseEntitiesL, true, ">",    false, "")]
    [DataRow("NonexistingField", UseEntitiesL, true, ">",    false, "")]

    [DataTestMethod]
    public void ValueSort_SortField_Basic_Ca10x(string field, bool useTable, bool multiLanguage, string direction, bool testAscending,  string notes)
    {
        var vf = _valueFilterMaker.GeneratePersonSourceWithDemoData(TestVolume, useTable, multiLanguage);
        vf.Attributes = field;
        vf.Directions = direction;
        var result = vf.ListTac().ToList();
        // check that each following city is same or larger...
        ValidateFieldIsSorted(result, field, testAscending);
    }

    // Now data that really changes direction based on language
    [DataRow(EnUs, "asc", MlSortWomanFirst, "First ML test")]
    [DataRow(FrFr, "asc", MlSortWomanFirst, "FR isn't defied, so it should use EN where she is first")]
    [DataRow(DeDe, "asc", MlSortMalesFirst, "German should result in him first")]
    [DataTestMethod]
    public void ValueSort_SortField_RealMl_Ca10x(string language, string direction, string expectedResults,  string notes)
    {
        var vf = _valueFilterMaker.GeneratePersonSourceWithDemoData(TestVolume, false, true);
        vf.Attributes = FieldBioForMlSortTest;
        vf.Directions = direction;
        vf.Languages = language;
        var result = vf.ListTac().ToList();
        // check that each following city is same or larger...
        ValidateFieldIsSorted(result, FieldIsMale,
            // the expected result is reversed, because string "True" is after string "False"
            expectedResults != MlSortMalesFirst
        );
    }

    [TestMethod]
    public void ValueSort_MultiField()
    {
        var vf = _valueFilterMaker.GeneratePersonSourceWithDemoData(TestVolume, true, false);
        vf.Attributes = FieldCity + "," + FieldIsMale;
        // vf.Directions = direction;
        var result = vf.ListTac().ToList();

        // TODO: VERIFY RESULT

        // check that each following city is same or larger...
        //ValidateFieldIsSorted(result, field, testAscending);
    }


    private void ValidateFieldIsSorted(List<IEntity> list, string field, bool asc)
    {
        var previous = list.First().TacGet<string>(field);
        foreach (var entity in list)
        {
            var next = entity.TacGet<string>(field);
            var comp = string.Compare(previous, next, StringComparison.Ordinal);
            if (asc)
                Assert.IsTrue(comp < 1, "new " + field + " " + next + " should be = or larger than prev " + previous);
            else
                Assert.IsTrue(comp > -1, "new " + field + " " + next + " should be = or smaller than prev " + previous);
            previous = next;
        }            
    }


}