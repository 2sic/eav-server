﻿using ToSic.Eav.DataSources.ValueFilter;

namespace ToSic.Eav.DataSourceTests;
// Todo
// Create tests with language-parameters as well, as these tests ignore the language and always use default

[Startup(typeof(StartupTestsValueFilter))]
public class ValueFilterNumbers(ValueFilterMaker valueFilterMaker)
{
    private const int TestVolume = 10000;
    private readonly ValueFilter _testDataGeneratedOutsideTimer = valueFilterMaker.CreateValueFilterForTesting(TestVolume, true);


    #region Number Filters

    [Fact]
    public void ValueFilter_FilterNumber()
        =>NumberFilter("Height", (PersonSpecs.MinHeight + 7).ToString(), 181);
        

    [Fact]
    public void ValueFilter_FilterNumberNone()
        =>NumberFilter("Height", "72", 0);
        

    [Fact]
    public void ValueFilter_FilterNumberEq()
        =>NumberFilter("Height", "182", 182, "==");


    [Fact]
    public void ValueFilter_FilterNumberEq2()
        =>NumberFilter("Height", "182", 182, "===");
        

    [Fact]
    public void ValueFilter_FilterNumberGt()
        => NumberFilter("Height", "180", 4368, ">");
        

    [Fact]
    public void ValueFilter_FilterNumberGtEq()
        => NumberFilter("Height", "180", 4550, ">=");
        

    [Fact]
    public void ValueFilter_FilterNumberLt()
        => NumberFilter("Height", "180", 5450, "<");
        

    [Fact]
    public void ValueFilter_FilterNumberLtEq()
        => NumberFilter("Height", "180", 5632, "<=");

    [Fact]
    public void ValueFilter_FilterNumberNotEq()
        => NumberFilter("Height", "180", 9818, "!=");

    public void ValueFilter_EntityId()
        => NumberFilter(EntityIdPascalCase, "9818", 9818, "==");

    [Fact]
    public void Between()
        => NumberFilter("Height", "175 and 185", 2002, "between");

    [Fact]
    public void BetweenNot()
        => NumberFilter("Height", "175 and 185", 10000-2002, "!between");

    public void NumberFilter(string attr, string value, int expected, string operation = null)
    {
        var vf = PrepareNumberFilterDs(attr, value, operation);
        Equal(expected, vf.ListTac().Count());//, "Should find exactly " + expected + " amount people");
    }

    private ValueFilter PrepareNumberFilterDs(string attr, string value, string operation)
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = attr;
        vf.Value = value;
        if (operation != null) vf.Operator = operation;
        return vf;
    }

    #endregion


    [Fact]
    public void NumberFilterInvalidOperator()
        => DataSourceErrors.VerifyStreamIsError(PrepareNumberFilterDs("Height", "180", "!!"), 
            CompareOperators.ErrorInvalidOperator);


}