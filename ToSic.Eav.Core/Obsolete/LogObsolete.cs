//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;
//using ToSic.Lib.Logging;
//using ToSic.Eav.Plumbing;
//using ToSic.Lib.Services;
//using static ToSic.Eav.Obsolete.CodeChangeConstants;

//namespace ToSic.Eav.Obsolete
//{
//    /// <summary>
//    /// Special logger to log obsolete messages
//    /// </summary>
//    public class LogObsolete: ServiceBase
//    {

//        ///// <summary>
//        ///// 
//        ///// </summary>
//        ///// <param name="change"></param>
//        ///// <param name="useCaseId">Exact use case. If empty, will try to use stack-info.</param>
//        ///// <param name="addMore">Additional things to log.</param>
//        //public LogObsolete(ICodeChangeInfo change, string useCaseId, Action<ILog> addMore) : base(EavLogs.Eav + ".Obsolt")
//        //{
//        //    try
//        //    {
//        //        var stackInfo = GetStackAndMainFile();

//        //        // If we don't have a case-identifier, use the path of the top CSHTML as the identifier
//        //        if (useCaseId.IsEmpty() && stackInfo.Main != MainError)
//        //            useCaseId = stackInfo.Main;

//        //        // Count how often this case has already been logged
//        //        var thing = change.NameId;
//        //        ObsoleteIdCount.TryGetValue(thing, out var countGeneral);
//        //        ObsoleteIdCount[thing] = ++countGeneral;

//        //        var countSpecific = 0;
//        //        var longId = thing + ":" + useCaseId;
//        //        if (useCaseId.HasValue())
//        //        {
//        //            // Count how often this case has already been logged
//        //            SpecificCases.TryGetValue(longId, out countSpecific);
//        //            SpecificCases[longId] = ++countSpecific;
//        //        }

//        //        // Don't log to normal warnings if it's been reported already
//        //        if (countGeneral > MaxGeneralToLog || countSpecific > MaxSpecificToLog) return;

//        //        var msg = $"Obsolete: {longId} is deprecated in v{change.From} "
//        //                  + (change.To == null ? "and has been removed." : $"and will be removed in {change.To}.");
//        //        Log.A(msg);
//        //        Log.A($"For further information, check: {change.Link}");

//        //        try
//        //        {
//        //            if (addMore != null)
//        //            {
//        //                Log.A("Additional Info:");
//        //                addMore.Invoke(Log);
//        //            }
//        //            else
//        //                Log.A("No additional info.");
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            Log.A("Error logging additional info.");
//        //            Log.Ex(ex);
//        //        }

//        //        // Try to add the stack trace
//        //        try
//        //        {
//        //            Log.A("Will now list Razor files in StackTrace");
//        //            foreach (var razorFile in stackInfo.AllCshtml)
//        //                Log.A(razorFile);

//        //            Log.A("Entire Stack for additional debugging");
//        //            Log.A(stackInfo.Stack);
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            Log.A("Error logging stack trace.");
//        //            Log.Ex(ex);
//        //        }

//        //        // Stop logging a general case if it's already been logged very often
//        //        if (countGeneral == MaxGeneralToLog)
//        //            Log.A($"This is the last log we'll add for the case '{thing}', further messages won't be logged for this problem.");

//        //        // Stop logging a specific case if it has already been logged
//        //        if (countSpecific == MaxSpecificToLog)
//        //            Log.A($"This is the only log we'll add for the id '{longId}', further messages won't be logged for this.");


//        //        new LogStoreLive().ForceAdd(ObsoleteNameInHistory, Log);
//        //    }
//        //    catch
//        //    {
//        //        /* ignore - avoid throwing errors just because logging is defect */
//        //    }
//        //}

//        //private (string Stack, string Main, string[] AllCshtml) GetStackAndMainFile()
//        //{
//        //    var stack = Environment.StackTrace;
//        //    try
//        //    {
//        //        // Note: in the stack, all relevant entries start with "Execute() in " and the path to the .cshtml
//        //        var razorFiles = Regex.Matches(stack, @"Execute\(\).*?\.cshtml");
//        //        var all = razorFiles
//        //            .Cast<Match>()
//        //            .Select(m => m.Value.Replace("Execute() in ", ""))
//        //            .ToArray();
//        //        return (stack, all.FirstOrDefault(), all);
//        //    }
//        //    catch
//        //    {
//        //        return (stack, MainError, Array.Empty<string>());
//        //    }
//        //}

//        //private static readonly Dictionary<string, int> ObsoleteIdCount = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

//        //private static Dictionary<string, int> SpecificCases =
//        //    new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
//    }
//}
