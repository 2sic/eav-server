using ToSic.Eav.WebApi.Sys.Helpers.Http;

namespace ToSic.Eav.WebApi.Sys.Helpers.Validation;

public abstract class ValidatorBase(ILog parentLog, string logName) : HelperBase(parentLog, logName)
{
    public string Errors = string.Empty;

    /// <summary>
    /// Determine if errors exist, and return that state
    /// </summary>
    /// <returns></returns>
    public static HttpExceptionAbstraction? BuildExceptionIfHasIssues(string errors, ILog? log, string? logMessage = null)
    {
        var l = log.Fn<HttpExceptionAbstraction?>();
        var hasErrors = errors != "";
        var preparedException = hasErrors ? HttpException.BadRequest(errors) : null;
        if (logMessage != null)
            l.A($"{nameof(logMessage)}:{logMessage}");
        if (hasErrors)
            l.A($"Errors:{errors}");
        return l.Return(preparedException, hasErrors ? "found errors" : "all ok");
    }


    /// <summary>
    /// Add an error message
    /// </summary>
    /// <param name="addition"></param>
    protected void Add(string addition)
    {
        var l = Log.Fn($"Add problem to list:{addition}");
        Errors += (Errors == string.Empty ? "" : "\n") + addition;
        l.Done();
    }
}