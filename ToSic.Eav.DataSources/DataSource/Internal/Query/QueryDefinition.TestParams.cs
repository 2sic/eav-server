using System.Text.RegularExpressions;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSource.Internal.Query;

partial class QueryDefinition
{
    internal const string KeyToken = "Token",
        KeyProperty = "Property",
        KeyValue = "Value";

    internal static Regex TestParamRegex = new(
        $@"(?:\[(?<{KeyToken}>\w+):(?<{KeyProperty}>\w+)\])=(?<{KeyValue}>[^\r\n]*)", RegexOptions.Compiled);



    /// <summary>
    /// The test parameters as stored in the IEntity
    /// </summary>
    public string TestParameters => Get<string>(FieldTestParams, null);

    [PrivateApi]
    public List<ILookUp> TestParameterLookUps => GenerateTestValueLookUps();

    /// <summary>
    /// Retrieve test values to test a specific query. 
    /// The specs are found in the query definition, but the must be converted to a source
    /// They are in the format [source:key]=value
    /// </summary>
    /// <returns></returns>
    private List<ILookUp> GenerateTestValueLookUps() => Log.Func($"{Entity.EntityId}", l =>
    {
        // Parse Test-Parameters in Format [Token:Property]=Value
        var testParameters = TestParameters;
        if (testParameters == null)
            return ([], "no test params");

        // extract the lines which look like [source:property]=value
        var testValueTokens = TestParamRegex.Matches(testParameters);

        // Create a list of static Property Accessors
        var result = new List<ILookUp>();
        foreach (Match testParam in testValueTokens)
        {
            var token = testParam.Groups[KeyToken].Value.ToLowerInvariant();

            // Ensure a PropertyAccess exists for this LookUp name
            if (!(result.FirstOrDefault(i => i.Name == token) is LookUpInDictionary currentLookUp))
            {
                currentLookUp = new(token);
                result.Add(currentLookUp);
            }

            // Add the static value
            currentLookUp.Properties.Add(testParam.Groups[KeyProperty].Value, testParam.Groups[KeyValue].Value);
        }

        return (result, "ok");
    });

}