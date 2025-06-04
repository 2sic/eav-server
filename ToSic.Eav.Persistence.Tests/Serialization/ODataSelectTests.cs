using ToSic.Eav.Serialization.Sys;

namespace ToSic.Eav.Serialization;

public class ODataSelectTests
{

    internal Dictionary<string, List<string>> GetFieldsByPrefixTac(List<string> fields)
        => ODataSelect.GetFieldsByPrefix(fields);

    internal Dictionary<string, List<string>> GetFieldsByPrefixTac(string? fields)
        => ODataSelect.GetFieldsByPrefix(fields == null ? [] : [..fields.Split(',')]);

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData(" ", 0)]
    [InlineData("id", 1, "1")]
    [InlineData("id,guid", 1, "2")]
    [InlineData("id,guid,FirstName", 1, "3")]
    [InlineData("id,guid,FirstName,LastName", 1, "4")]
    [InlineData("id,guid,Child/FirstName,Child/LastName,Address", 2, "3,2")]
    [InlineData("id,guid,Child/FirstName,Child/LastName,Child/Xyz,Child2/Address", 3, "2,3,1")]
    public void Test(string original, int mainCount, string partsCount = "")
    {
        // Count how many dictionary are returned
        var parts = GetFieldsByPrefixTac(original);
        Equal(mainCount, parts.Count);

        // Combine the counts of each dictionary to do a quick check
        var subParts = string.Join(",", parts.Select(p => p.Value.Count));
        Equal(partsCount, subParts);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("id", true)]
    [InlineData("id,guid", true)]
    [InlineData("id,guid,FirstName", true)]
    [InlineData("id,Test/guid,Test/FirstName", true)]
    [InlineData("id,Test/guid,Test/FirstName", true, "Test")]
    [InlineData("Test/id,Test/guid,Test/FirstName", false)]
    [InlineData("Test/id,Test/guid,Test/FirstName", true, "Test")]
    [InlineData("Test/id,Test/guid,Test/FirstName", false, "Invalid")]
    public void Test2(string original, bool hasList, string part = ODataSelect.Main)
    {
        var parts = new ODataSelect(original == null ? null : original.Split(',').ToList());
        Equal(hasList, parts.GetFieldsOrNull(part) != null);
    }
}

