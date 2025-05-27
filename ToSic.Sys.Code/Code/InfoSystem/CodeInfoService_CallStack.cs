using System.Diagnostics;
using System.Text.RegularExpressions;
using static ToSic.Lib.Code.InfoSystem.CodeInfoConstants;

namespace ToSic.Lib.Code.InfoSystem;

partial class CodeInfoService
{
    private const int StackFramesWhichBelongToThisService = 3;

    private (string Stack, string? Main, string[] AllCshtml) GetStackAndMainFile()
    {
        // Get stack trace skipping some frames which are part of the logging infrastructure
        var trace = new StackTrace(StackFramesWhichBelongToThisService, true);
        var stack = trace.ToString();
        try
        {
            // Note: in the stack, all relevant entries start with "Execute() in " and the path to the .cshtml
            var razorFiles = Regex.Matches(stack, @"Execute\(\).*?\.cshtml");
            var all = razorFiles
                .Cast<Match>()
                .Select(m => m.Value.Replace("Execute() in ", ""))
                .ToArray();
            return (stack, all.FirstOrDefault(), all);
        }
        catch
        {
            return (stack, MainError, []);
        }
    }

}