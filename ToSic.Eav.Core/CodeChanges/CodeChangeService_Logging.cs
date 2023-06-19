using System;
using System.Collections.Generic;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;
using static ToSic.Eav.CodeChanges.CodeChangeConstants;
using static System.StringComparer;

namespace ToSic.Eav.CodeChanges
{
    public partial class CodeChangeService
    {
        /// <summary>
        /// Keep track of all obsolete code, so we don't over-report it.
        /// </summary>
        private static Dictionary<string, int> ObsoleteIdCount { get; } = new Dictionary<string, int>(InvariantCultureIgnoreCase);

        /// <summary>
        /// Keep track of specific cases of an obsolete report, because these should always only be reported once
        /// </summary>
        private static Dictionary<string, int> SpecificCases { get; } = new Dictionary<string, int>(InvariantCultureIgnoreCase);

        private static readonly EntryOptions NoCodeDetails = new EntryOptions { HideCodeReference = true };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="change"></param>
        private CodeChangeLogged LogObsolete(CodeChangeUse change)
        {
            var l = Log.Fn<CodeChangeLogged>($"Obsolete: '{change.NameId}' will be removed {(change.To == null ? null: "v" + change.To)}");
            CodeChangeLogged changeLogged = null;
            try
            {
                var stackInfo = GetStackAndMainFile();

                // If we don't have a case-identifier, use the path of the top CSHTML as the identifier
                var useCaseId = change.SpecificId;
                if (useCaseId.IsEmpty() && stackInfo.Main != MainError)
                    useCaseId = stackInfo.Main;

                // New 16.02 - on WebAPIs we should have the entry point to mark the specific use
                if (useCaseId.IsEmpty())
                    useCaseId = _scope.Value.EntryPoint;

                // Count how often this case has already been logged
                var thing = change.NameId;
                ObsoleteIdCount.TryGetValue(thing, out var countGeneral);
                ObsoleteIdCount[thing] = ++countGeneral;

                var countSpecific = 0;
                var longId = thing + ":" + useCaseId;
                if (useCaseId.HasValue())
                {
                    // Count how often this case has already been logged
                    SpecificCases.TryGetValue(longId, out countSpecific);
                    SpecificCases[longId] = ++countSpecific;
                }

                // Don't log to normal warnings if it's been reported already
                if (countGeneral > MaxGeneralToLog || countSpecific > MaxSpecificToLog) return l.Return(new CodeChangeLogged(change));

                // Now we know that we'll keep it
                var logEntry = new LogStoreLive().ForceAdd(ObsoleteNameInHistory, Log);
                changeLogged = new CodeChangeLogged(change, logEntry);

                var msg = $"Obsolete: {longId} is deprecated in v{change.From} "
                          + (change.To == null ? "and has been removed." : $"and will be removed in {change.To}.");
                l.A(msg, options: NoCodeDetails);
                l.A(change.Link.HasValue() ? $"For further information, check: {change.Link}" : "No link for further help provided.", options: NoCodeDetails);

                try
                {
                    if (change.More != null)
                    {
                        l.A("Additional Info:", options: NoCodeDetails);
                        foreach (var s in change.More)
                            l.A(s, options: new EntryOptions { ShowNewLines = true, HideCodeReference = true });
                    }
                    else
                        l.A("No additional info.", options: NoCodeDetails);
                }
                catch (Exception ex)
                {
                    l.Ex("Error logging additional info.", ex);
                }

                // Try to add the stack trace
                try
                {
                    l.A("Will now list Razor files in StackTrace");
                    foreach (var razorFile in stackInfo.AllCshtml)
                        l.A(razorFile, options: NoCodeDetails);

                    l.A("Entire Stack for additional debugging \n" + stackInfo.Stack, options: new EntryOptions { ShowNewLines = true, HideCodeReference = true});
                }
                catch (Exception ex)
                {
                    l.Ex("Error logging stack trace.", ex);
                }

                // Stop logging a general case if it's already been logged very often
                if (countGeneral == MaxGeneralToLog)
                    l.A($"This is the last log we'll add for the case '{thing}', further messages won't be logged for this problem.", options: NoCodeDetails);

                // Stop logging a specific case if it has already been logged
                if (countSpecific == MaxSpecificToLog)
                    l.A($"This is the only log we'll add for the id '{longId}', further messages won't be logged for this.", options: NoCodeDetails);

            }
            catch
            {
                /* ignore - avoid throwing errors just because logging is defect */
            }

            return l.Return(changeLogged ?? new CodeChangeLogged(change));
        }
    }
}
