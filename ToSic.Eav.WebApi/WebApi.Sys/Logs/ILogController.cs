namespace ToSic.Eav.WebApi.Sys.Logs;

public interface ILogController
{
    /// <summary>
    /// Used to be GET System/ExtendedLogging
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="activateForDuration"></param>
    /// <returns></returns>
    string EnableDebug(int duration = 1);
}