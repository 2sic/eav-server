using ToSic.Eav.DataSources.ValueFilter;
using static ToSic.Eav.DataSources.CompareOperators;
using static ToSic.Eav.TestData.PersonSpecs;
#pragma warning disable xUnit1026

namespace ToSic.Eav.DataSourceTests;
// Todo
// Create tests with language-parameters as well, as these tests ignore the language and always use default

[Startup(typeof(StartupTestsValueFilter))]
public class ValueFilterDateTime(ValueFilterMaker valueFilterMaker)
{
    private const int TestVolume = 10000;
    private readonly ValueFilter _testDataGeneratedOutsideTimer = valueFilterMaker.CreateValueFilterForTesting(TestVolume, true);


    #region DateTime Filters

    [Theory]
    [InlineData(FieldBirthday, "1990-01-01", 2, null, "EqIso")]
    [InlineData(FieldBirthday, "4/4/1903", 2, null, "EqUsa")]
    [InlineData(FieldBirthday, "6.6.1905", 2, null, "EqEurope")]
    [InlineData("Modified", "6.6.1905", 0, null, "DateModifiedZero")]
    [InlineData("Modified", "2100-01-01", 10000, OpLt, "DateModifiedOlder")]
    [InlineData("Modified", "0000-01-01 and 2100-01-01", 10000, "Between", "DateModifiedBetween")]
    [InlineData(FieldBirthday, "24.8.1997", 1124, OpGt, "Gt")]// this is one of the generated dates
    [InlineData(FieldBirthday, "24.8.1997", 10000 - 1127, OpLt, "Lt")]// this is one of the generated dates
    [InlineData(FieldBirthday, "24.8.1997", 1127, OpGtEquals, "GtEq")] // this is one of the generated dates
    [InlineData(FieldBirthday, "24.8.1997", 10000 - 1124, OpLtEquals, "LtEq")] // this is one of the generated dates
    [InlineData(FieldBirthday, "24.8.1997", 3, OpExactly, "Eq1997")] // this is one of the generated dates
    [InlineData(FieldBirthday, "24.8.1997", 10000 - 3, OpNotEquals, "NotEq1997")] // this is one of the generated dates
    [InlineData(FieldBirthday, "1.1.1995 and 31.12.2000", 546, OpBetween, "Between")] // this is one of the generated dates
    [InlineData(FieldBirthday, "1.1.1995 and 31.12.2000", 10000 - 546, OpNotBetween, "BetweenNot")] // this is one of the generated dates
    [InlineData(FieldBirthday, "1.1.1995 and 31.12.2000", 10000 - 546, OpNotBetween, "BetweenNot")] // this is one of the generated dates
    [InlineData(FieldBirthdayNull, "", 10000 / 2, OpEquals, "eq empty")] // this is one of the generated dates
    public void DateTimeFilter(string attr, string value, int expected, string operation, string name)
    {
        var vf = PrepareDateTimeFilterDs(attr, value, operation);
        var list = vf.ListTac().ToList();
        Equal(expected, list.Count());//, $"{name}: find exactly " + expected + " amount people");
    }

    private ValueFilter PrepareDateTimeFilterDs(string attr, string value, string operation)
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = attr;
        vf.Value = value;
        if (operation != null) vf.Operator = operation;
        return vf;
    }

    #endregion

    #region DateTime Null filter

    [Theory]
    [InlineData("1.1.1995 and 31.12.2000", 273, OpBetween, "NullBetween")]
    [InlineData("1.1.1995 and 31.12.2000", 9727, OpNotBetween, "NullBetweenNot")]
    [InlineData(" and 31.12.2000", 9636, OpBetween, "NullBetweenNullAndReal")]
    [InlineData("", 10000 / 2, OpEquals, "Null ==")]
    [InlineData("", 10000 / 2, OpNotEquals, "Null !=")]
    [InlineData("24.8.1997", 546, OpGt, "Null >")]
    [InlineData("24.8.1997", 9454, OpLt, "Null <")]
    [InlineData("24.8.1997", 546, OpGtEquals, "Null >=")]
    [InlineData("24.8.1997", 9454, OpLtEquals, "Null <=")]
    public void FilterNullBDay(string value, int expected, string operation, string name = null)
        => DateTimeFilter("BirthdateMaybeNull", value, expected, operation, null);

    #endregion


    [Fact]
    public void NumberFilterInvalidOperator()
        => DataSourceErrors.VerifyStreamIsError(PrepareDateTimeFilterDs(FieldBirthday, "180", "!!"),
            ErrorInvalidOperator);


}