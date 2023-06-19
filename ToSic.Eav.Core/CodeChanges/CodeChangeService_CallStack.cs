using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using static ToSic.Eav.CodeChanges.CodeChangeConstants;

namespace ToSic.Eav.CodeChanges
{
    public partial class CodeChangeService
    {
        private const int StackFramesWhichBelongToThisService = 3;

        private (string Stack, string Main, string[] AllCshtml) GetStackAndMainFile()
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
                return (stack, MainError, Array.Empty<string>());
            }
        }

    }
}
