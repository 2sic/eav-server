using System.Text.RegularExpressions;

namespace ToSic.Eav.LookUp.Tests.Tokens;
internal static class TokenTestHelpers
{
    /// <summary>
    /// Helper method to count how many times a certain type of result was returned
    /// </summary>
    /// <param name="results"></param>
    /// <param name="resultName"></param>
    /// <returns></returns>
    internal static int CountSpecificResultTypes(this MatchCollection results, string resultName) =>
        results.Cast<Match>().Count(match => !string.IsNullOrEmpty(match.Result(resultName)));

}
