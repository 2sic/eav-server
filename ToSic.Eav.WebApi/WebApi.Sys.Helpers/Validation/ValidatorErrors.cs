using ToSic.Eav.WebApi.Sys.Helpers.Http;

namespace ToSic.Eav.WebApi.Sys.Helpers.Validation;

public static class ValidatorErrors
{

    /// <summary>
    /// Determine if errors exist, and return that state
    /// </summary>
    /// <returns></returns>
    public static Exception? BuildExceptionIfHasIssues(string errors, ILog? log, string? logMessage = null)
    {
        var l = log.Fn<Exception?>();
        var hasErrors = errors != "";
        var preparedException = hasErrors ? new Exception(errors) : null;
        if (logMessage != null)
            l.A($"{nameof(logMessage)}:{logMessage}");
        if (hasErrors)
            l.A($"Errors:{errors}");
        return l.Return(preparedException, hasErrors ? "found errors" : "all ok");
    }
}
