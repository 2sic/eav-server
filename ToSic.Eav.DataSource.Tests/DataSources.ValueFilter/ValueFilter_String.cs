using ToSic.Eav.DataSources.Internal;
using static ToSic.Eav.DataSource.DataSourceConstants;
using static ToSic.Eav.DataSources.CompareOperators;
using static ToSic.Eav.TestData.PersonSpecs;
using static ToSic.Eav.DataSourceTests.ValueSortShared;
using ToSic.Eav.DataSources.ValueFilter;

namespace ToSic.Eav.DataSourceTests;
// Todo
// Create tests with language-parameters as well, as these tests ignore the language and always use default

[Startup(typeof(StartupValueFilter))]
public class ValueFilterString(ValueFilterMaker valueFilterMaker)
{
    private const int TestVolume = 10000;
    private readonly ValueFilter _testDataGeneratedOutsideTimer = valueFilterMaker.CreateValueFilterForTesting(TestVolume, true);

    private ValueFilter GetFilter(bool table, bool ml, string field, string compare = null, string value = null)
    {
        var filter = valueFilterMaker.CreateValueFilterForTesting(SmallVolume, table, ml);
        filter.Attribute = field;
        if (compare != null) filter.Operator = compare;
        if (value != null) filter.Value = value;
        return filter;
    }

    [Theory]
    [InlineData(true, false, City1, Quarter, null, "use table for first test because that always worked")]
    [InlineData(false, false, City1, Quarter, null, "use entities, but not ML yet")]
    [InlineData(false, true, City1, None, null, "simple ML case, won't find")]
    [InlineData(false, true, PriPrefix + City1, Quarter, null, "find primary language because no lang specified")]
    [InlineData(false, true, EnPrefix + City1, None, null, "look for no lang, so shouldn't find the EN entries")]
    [InlineData(false, true, PriPrefix + City1, Quarter, null, "no lang, should find Pri")]
    [InlineData(false, true, EnPrefix + City1, None, null, "no lang, shouldn't find EN")]
    [InlineData(false, true, EnPrefix + City1, Quarter, EnUs, "Look for en, should find")]
    [InlineData(false, true, PriPrefix + City1, Quarter, ValueLanguages.LanguageDefaultPlaceholder, "Look for pri, should find")]
    [InlineData(false, true, EnPrefix + City1, None, ValueLanguages.LanguageDefaultPlaceholder, "Pri - shouldn't find en")]
    [InlineData(false, true, PriPrefix + City1, Quarter, "", "none - should find pri")]
    [InlineData(false, true, EnPrefix + City1, None, "", "none, shouldn't find en")]
    [InlineData(false, true, PriPrefix + City1, None, ValueLanguages.LanguageCurrentPlaceholder, "current, shouldn't find pri")]
    [InlineData(false, true, EnPrefix + City1, Quarter, ValueLanguages.LanguageCurrentPlaceholder, "current = en should find EN")]
    [InlineData(false, true, PriPrefix + City1, Quarter, En, "'en' will find the primary ")]
    [InlineData(false, true, EnPrefix + City1, None, En, "Just 'en' can't find any")]
    [InlineData(false, true, EnPrefix + City1, None, FrFr, "fr shouldn't find EN")]
    [InlineData(false, true, FrPrefix + City1, None, null, "none shouldn't find FR")]
    [InlineData(false, true, FrPrefix + City1, None, EnUs, "EN shouldn't find FR")]
    [InlineData(false, true, FrPrefix + City1, Quarter, FrFr, "FR should find fr")]
    [InlineData(false, true, FrPrefix + City1, None, Fr, "just 'fr' shouldn't find anything")]
    [InlineData(false, true, PriPrefix + City1, Quarter, "xx", "unknown should find default/pri")]
    [InlineData(false, true, EnPrefix + City1, None, "xx", "unknown shouldn't find en")]
    public void ValueFilter_SimpleText_MultiLang_Ca25x(bool table, bool ml, string city, int expected, string lang, string notes)
    {
        var vf = GetFilter(table, ml, FieldCity, value: city);
        if (lang != null) vf.Languages = lang;
        Equal(expected, vf.ListTac().Count());//, $"Should find exactly {expected} people with this city");
    }

    [Fact]
    public void ValueFilter_SimpleTextFilterCIDefault()
    {
        var vf = GetFilter(true, false, FieldCity);
        vf.Value = City1.ToLowerInvariant(); // test for the first value
        Equal(Quarter, vf.ListTac().Count());//, "Should find exactly 2500 people with this city");
    }

    #region String Case Sensitive
    [Fact]
    public void ValueFilter_SimpleTextFilterCSWithResults()
    {
        var vf = GetFilter(true, false, FieldCity, "===", City1);
        Equal(Quarter, vf.ListTac().Count());//, "Should find exactly 2500 people with this city");
    }       

    [Fact]
    public void ValueFilter_SimpleTextFilterCSWithoutResults()
    {
        var vf = GetFilter(true, false, FieldCity, "===");
        //vf.Operator = "===";
        vf.Value = City1.ToLowerInvariant(); // test for the first value
        Equal(0, vf.ListTac().Count());//, "Should find exactly 0 people with this city");
    }

    [Fact]
    public void ValueFilter_SimpleTextFilterCSWithSomeNulls()
    {
        var vf = GetFilter(true, false, FieldCityMaybeNull, "===");
        //vf.Attribute = "CityMaybeNull";
        //vf.Operator = "===";
        vf.Value = "Grabs"; // test for the first value
        Equal(Quarter, vf.ListTac().Count());//, "Should find exactly 0 people with this city");
    }

    #endregion

    #region "all"
    [Fact]
    public void ValueFilter_All()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        vf.Operator = OpAll;
        vf.Value = "uCHs";
        Equal(10000, vf.ListTac().Count());//, "Should find exactly 10000 people with this city");
    }
    #endregion

    #region String contains

    [Fact]
    public void ValueFilter_TextContainsWithResults()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        vf.Operator = OpContains;
        vf.Value = "uCHs";
        Equal(2500, vf.ListTac().Count());//, "Should find exactly 2500 people with this city");
    }

    #endregion

    #region Take-tests and "all"

    [Fact]
    public void ValueFilter_TextContainsOneOnly()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        vf.Operator = OpContains;
        vf.Value = "uCHs";
        vf.Take = "5";
        Equal(5, vf.ListTac().Count());//, "Should find exactly 5 people with this city");
    }
    [Fact]
    public void ValueFilter_TakeContainsCH1000()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        vf.Operator = OpContains;
        vf.Value = "CH";
        vf.Take = "1000";
        Equal(1000, vf.ListTac().Count());//, "Should find exactly 5 people with this city");
    }

    [Fact]
    public void ValueFilter_TakeAll10000()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        vf.Operator = OpAll;
        vf.Value = "uCHs";
        vf.Take = "10000";
        Equal(10000, vf.ListTac().Count());//, "Should find exactly 5 people with this city");
    }        
    [Fact]
    public void ValueFilter_TakeNone()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        vf.Operator = OpNone;
        vf.Value = "uCHs";
        vf.Take = "10000";
        Equal(0, vf.ListTac().Count());//, "Should find none");
    }        

    [Fact]
    public void ValueFilter_TakeAll90000()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        vf.Operator = OpAll;
        vf.Value = "uCHs";
        vf.Take = "90000";
        Equal(10000, vf.ListTac().Count());//, "Should find exactly 5 people with this city");
    }        
    #endregion

    #region String begins

    [Fact]
    public void ValueFilter_SimpleTextFilterBegins()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        vf.Operator = "begins";
        vf.Value = "bu";
        Equal(2500, vf.ListTac().Count());//, "Should find exactly 2500 people with this city");
    }
    [Fact]
    public void ValueFilter_SimpleTextFilterBeginsNone()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "CityMaybeNull";
        vf.Operator = "begins";
        vf.Value = "St.";
        Equal(0, vf.ListTac().Count());//, "Should find exactly 0 people with this city");
    }
    #endregion

    #region String Contains / Not Contains
    [Fact]
    public void ValueFilter_SimpleTextFilterContainsWithoutResults()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        vf.Operator = OpContains;
        vf.Value = "Buchs SG";
        Equal(0, vf.ListTac().Count());//, "Should find exactly 0 people with this city");
    }

    [Fact]
    public void ValueFilter_SimpleTextFilterNotContains()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        vf.Operator = "!contains";
        vf.Value = "ch";
        Equal(5000, vf.ListTac().Count());//, "Should find exactly 5000 people with this city");
    }
    #endregion


    [Fact]
    public void ValueFilter_SimpleTextFilterWithoutResults()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        vf.Value = "Daniel";
        Equal(0, vf.ListTac().Count());//, "Should find exactly 0 people with this city");
    }
        
    [Fact]
    public void ValueFilter_FilterOnUnexistingProperty()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "ZIPCodeOrSomeOtherNotExistingField";
        vf.Value = "9470"; // test for the first value
        Equal(0, vf.ListTac().Count());//, "Should find exactly 0 people with this city");
    }

    [Fact]
    public void ValueFilter_FilterFieldSometimesNull()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "CityMaybeNull";
        vf.Value = TestCities[1]; // test for the second value
        Equal(2500, vf.ListTac().Count());//, "Should find exactly 250 people with this city");
    }


    #region Fallback Tests
    [Fact]
    public void ValueFilter_WithoutFallback()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        // vf.Operator = "==";
        vf.Value = "inexisting city";
        Equal(0, vf.ListTac().Count());//, "Should find exactly 0 people with this city");
    }

    [Fact]
    public void ValueFilter_WithFallback()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attribute = "City";
        // vf.Operator = "==";
        vf.Value = "inexisting city";
            
        // attach fallback to give all if no match
        vf.Attach(StreamFallbackName, vf.InTac()[StreamDefaultName]);
        Equal(TestVolume, vf.ListTac().Count());//, "Should find exactly 0 people with this city");
    }
    #endregion

}