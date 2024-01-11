using ToSic.Eav.WebApi.Errors;

namespace ToSic.Eav.WebApi.Validation;

public abstract class ValidatorBase(ILog parentLog, string logName) : HelperBase(parentLog, logName)
{
    public string Errors = string.Empty;

    private bool HasErrors => Errors != string.Empty;

    /// <summary>
    /// Determine if errors exist, and return that state
    /// </summary>
    /// <returns></returns>
    protected bool BuildExceptionIfHasIssues(out HttpExceptionAbstraction preparedException, string logMessage = null)
    {
        var wrapLog = Log.Fn<bool>();
        preparedException = HasErrors ? HttpException.BadRequest(Errors): null;
        if (logMessage != null) Log.A($"{nameof(logMessage)}:{logMessage}");
        if (HasErrors) Log.A($"Errors:{Errors}");
        return wrapLog.Return(!HasErrors, HasErrors ? "found errors" : "all ok");
    }


    /// <summary>
    /// Add an error message
    /// </summary>
    /// <param name="addition"></param>
    protected void Add(string addition)
    {
        Log.A($"Add problem to list:{addition}");
        Errors += (Errors == string.Empty ? "" : "\n") + addition;
    }
}