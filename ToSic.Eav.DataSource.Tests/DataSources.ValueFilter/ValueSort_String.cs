using ToSic.Eav.DataSources.ValueFilter;
using static ToSic.Eav.TestData.PersonSpecs;

namespace ToSic.Eav.DataSourceTests;
// Todo
// 1. Tests with standard fields like EntityTitle
// 2. Tests with multiple fileds like City,Name
// Later: Create tests with language-parameters as well, as these tests ignore the language and always use default

[Startup(typeof(StartupTestsValueFilter))]
public class ValueSort_String(ValueFilterMaker valueFilterMaker)
{
    private const int TestVolume = 30;
    private readonly ValueSort _testDataGeneratedOutsideTimer = valueFilterMaker.GeneratePersonSourceWithDemoData(TestVolume);

    [Fact]
    public void ValueSort_Unused()
    {
        var vf = _testDataGeneratedOutsideTimer;
        var listOut = vf.ListTac().ToList();
        var listIn = vf.InTac()[DataSourceConstants.StreamDefaultName].ListTac().ToList();
        Assert.Equal(listOut, listIn);//, "Lists should be the same if no criteria applied");
    }

    [Fact]
    public void ValueSort_SortFieldOnly_City()
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attributes = FieldCity;
        var result = vf.ListTac().ToList();
        // check that each following city is same or larger...
        ValidateFieldIsSorted(result, FieldCity, true);
    }

    // Initial control test using table, not entities which could be ML
    [InlineData(FieldCity, UseDataTable, false, "asc", true, "")]

    // Basic tests single language
    [InlineData(FieldCity, UseEntitiesL, false, "",     true, "No direction should = asc.")]
    [InlineData(FieldCity, UseEntitiesL, false, "a",    true, "Sort city")]
    [InlineData(FieldCity, UseEntitiesL, false, "1",    true, "sort city")]
    [InlineData(FieldCity, UseEntitiesL, false, "<",    true, "")]
    [InlineData(FieldCity, UseEntitiesL, false, "desc", false, "")]
    [InlineData(FieldCity, UseEntitiesL, false, "d",    false, "")]
    [InlineData(FieldCity, UseEntitiesL, false, "D",    false, "")]
    [InlineData(FieldCity, UseEntitiesL, false, "0",    false, "")]
    [InlineData(FieldCity, UseEntitiesL, false, ">",    false, "")]

    // same tests now ML, results should be the same
    [InlineData(FieldCity, UseEntitiesL, true, "",     true, "No direction should = asc.")]
    [InlineData(FieldCity, UseEntitiesL, true, "a",    true, "Sort city")]
    [InlineData(FieldCity, UseEntitiesL, true, "1",    true, "sort city")]
    [InlineData(FieldCity, UseEntitiesL, true, "<",    true, "")]
    [InlineData(FieldCity, UseEntitiesL, true, "desc", false, "")]
    [InlineData(FieldCity, UseEntitiesL, true, "d",    false, "")]
    [InlineData(FieldCity, UseEntitiesL, true, "D",    false, "")]
    [InlineData(FieldCity, UseEntitiesL, true, "0",    false, "")]
    [InlineData(FieldCity, UseEntitiesL, true, ">",    false, "")]
    [InlineData("NonexistingField", UseEntitiesL, true, ">",    false, "")]

    [Theory]
    public void ValueSort_SortField_Basic_Ca10x(string field, bool useTable, bool multiLanguage, string direction, bool testAscending,  string notes)
    {
        var vf = valueFilterMaker.GeneratePersonSourceWithDemoData(TestVolume, useTable, multiLanguage);
        vf.Attributes = field;
        vf.Directions = direction;
        var result = vf.ListTac().ToList();
        // check that each following city is same or larger...
        ValidateFieldIsSorted(result, field, testAscending);
    }

    // Now data that really changes direction based on language
    [Theory]
    [InlineData(EnUs, "asc", MlSortWomanFirst, "First ML test")]
    [InlineData(FrFr, "asc", MlSortWomanFirst, "FR isn't defied, so it should use EN where she is first")]
    [InlineData(DeDe, "asc", MlSortMalesFirst, "German should result in him first")]
    public void ValueSort_SortField_RealMl_Ca10x(string language, string direction, string expectedResults,  string notes)
    {
        var vf = valueFilterMaker.GeneratePersonSourceWithDemoData(TestVolume, false, true);
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

    [Fact]
    public void ValueSort_MultiField()
    {
        var vf = valueFilterMaker.GeneratePersonSourceWithDemoData(TestVolume, true, false);
        vf.Attributes = FieldCity + "," + FieldIsMale;
        // vf.Directions = direction;
        var result = vf.ListTac().ToList();

        // TODO: VERIFY RESULT

        // check that each following city is same or larger...
        //ValidateFieldIsSorted(result, field, testAscending);
    }


    private void ValidateFieldIsSorted(List<IEntity> list, string field, bool asc)
    {
        var previous = list.First().GetTac<string>(field);
        foreach (var entity in list)
        {
            var next = entity.GetTac<string>(field);
            var comp = string.Compare(previous, next, StringComparison.Ordinal);
            if (asc)
                True(comp < 1, "new " + field + " " + next + " should be = or larger than prev " + previous);
            else
                True(comp > -1, "new " + field + " " + next + " should be = or smaller than prev " + previous);
            previous = next;
        }            
    }


}