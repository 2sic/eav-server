using ToSic.Eav.WebApi.Sys.Helpers.Http;

namespace ToSic.Eav.WebApi.Sys.Helpers.Validation;

public abstract class ValidatorBase(ILog parentLog, string logName) : HelperBase(parentLog, logName)
{
    public string Errors = string.Empty;

    private bool HasErrors => Errors != string.Empty;

    /// <summary>
    /// Determine if errors exist, and return that state
    /// </summary>
    /// <returns></returns>
    protected bool BuildExceptionIfHasIssues([NotNullWhen(true)] out HttpExceptionAbstraction? preparedException, string? logMessage = null)
    {
        var l = Log.Fn<bool>();
        preparedException = HasErrors ? HttpException.BadRequest(Errors): null;
        if (logMessage != null)
            l.A($"{nameof(logMessage)}:{logMessage}");
        if (HasErrors)
            l.A($"Errors:{Errors}");
        return l.Return(!HasErrors, HasErrors ? "found errors" : "all ok");
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