namespace ToSic.Eav.WebApi.Sys;
// Release routes

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LogControllerReal() : ServiceBase($"{EavLogs.WebApi}.{LogSuffix}Rl")
{
    public const string LogSuffix = "Log";

    public string EnableDebug(Func<int, string> activateForDuration, int duration = 1)
    {
        Log.A("Extended logging will set for duration:" + duration);
        var msg = activateForDuration(duration);
        Log.A(msg);
        return msg;
    }
}