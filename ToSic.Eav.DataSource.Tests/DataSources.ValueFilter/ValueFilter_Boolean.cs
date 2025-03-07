using ToSic.Eav.DataSources.ValueFilter;

namespace ToSic.Eav.DataSourceTests;
// Todo
// Create tests with language-parameters as well, as these tests ignore the language and always use default

[Startup(typeof(StartupValueFilter))]
public class ValueFilterBoolean(ValueFilterMaker valueFilterMaker)
{

    [Theory]
    [InlineData("True", 82, 82, true, "True - Table")]
    [InlineData("True", 82, 82, false, "True - Entity")]
    [InlineData("true", 82, 82, true, "true - Table")]
    [InlineData("true", 82, 82, false, "true - Entity")]
    [InlineData("TRUE", 82, 82, true, "TRUE - Table")]
    [InlineData("TRUE", 82, 82, false, "TRUE - Entity")]
    [InlineData("FALSE", 164, 82, true, "FALSE - Table")]
    [InlineData("FALSE", 164, 82, false, "FALSE - Entity")]
    [InlineData("false", 164, 82, true, "false - Table")]
    [InlineData("false", 164, 82, false, "false - Entity")]
    public void FilterBool(string compareValue, int desiredFinds, int populationRoot, bool useTable, string name)
    {
        var vf = valueFilterMaker.CreateValueFilterForTesting(populationRoot * PersonSpecs.IsMaleForEveryX, useTable); // only every 3rd is male in the demo data
        vf.Attribute = "IsMale";
        vf.Value = compareValue;
        var found = vf.ListTac().Count();
        Equal(desiredFinds, found); //, "Should find exactly this amount people");
    }

}