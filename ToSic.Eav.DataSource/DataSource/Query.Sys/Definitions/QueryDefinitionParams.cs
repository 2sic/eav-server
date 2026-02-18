using System.Text.RegularExpressions;
using ToSic.Eav.LookUp;
using ToSic.Eav.LookUp.Sources;

namespace ToSic.Eav.DataSource.Query.Sys;

internal class QueryDefinitionParams
{
    internal const string KeyToken = "Token",
        KeyProperty = "Property",
        KeyValue = "Value";

    /// <summary>
    /// Regex to detect key=value. <br/>
    /// Keys must always be the first thing optionally followed by a = and then anything till a newline.
    /// Anything that doesn't match will be ignored. <br/>
    /// Comments should start with a //
    /// </summary>
    internal static Regex ParamRegex = new(
        $@"^(?<{KeyProperty}>\w+)(\=(?<{KeyValue}>[^\r\n]*)?)?",
        RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    /// Generate the LookUp
    /// They are in the format k=value or key=[some:token]
    /// </summary>
    [PrivateApi]
    public static IDictionary<string, string> GenerateParamsDic(string? paramsText, ILog? log)
    {
        var l = log.Fn<IDictionary<string, string>>();

        var paramsDic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        if (string.IsNullOrWhiteSpace(paramsText))
            return l.Return(paramsDic, "no params");

        // extract the lines which look like key=value
        var paramMatches = ParamRegex.Matches(paramsText);
        l.A($"found {paramMatches.Count} params");

        foreach (Match testParam in paramMatches)
        {
            var key = testParam.Groups[KeyProperty].Value.ToLowerInvariant();
            var value = testParam.Groups[KeyValue].Value;
            l.A($"Params:{key}={value}");
            if (!paramsDic.ContainsKey(key))
                paramsDic[key] = value; // add not-yet-added-value
            else
                l.A($"Params:{key} already existed, will leave as is");
        }

        return l.Return(paramsDic, paramsDic.Count.ToString());
    }

    // Special Regex
    // Note that the keyProperty is now a-zA-Z0-9$_- to allow for more complex property names
    internal static Regex TestParamRegex = new(
        $@"(?:\[(?<{KeyToken}>\w+):(?<{KeyProperty}>[\w$-]+)\])=(?<{KeyValue}>[^\r\n]*)", RegexOptions.Compiled);


    /// <summary>
    /// Retrieve test values to test a specific query. 
    /// The specs are found in the query definition, but the must be converted to a source
    /// They are in the format [source:key]=value
    /// </summary>
    /// <returns></returns>
    internal static List<ILookUp> GenerateTestValueLookUps(string? testParameters, ILog? log = default)
    {
        var l = log.Fn<List<ILookUp>>();
        // Parse Test-Parameters in Format [Token:Property]=Value
        if (testParameters == null)
            return l.Return([], "no test params");

        // extract the lines which look like [source:property]=value
        var testValueTokens = TestParamRegex.Matches(testParameters);

        // Create a list of static Property Accessors
        var result = new List<ILookUp>();
        foreach (Match testParam in testValueTokens)
        {
            var token = testParam.Groups[KeyToken].Value.ToLowerInvariant();

            // Ensure a PropertyAccess exists for this LookUp name
            if (result.FirstOrDefault(i => i.Name == token) is not LookUpInDictionary currentLookUp)
            {
                currentLookUp = new(token);
                result.Add(currentLookUp);
            }

            // Add the static value
            currentLookUp.Properties.Add(testParam.Groups[KeyProperty].Value, testParam.Groups[KeyValue].Value);
        }

        return l.Return(result, "ok");
    }
}
