namespace ToSic.Eav.WebApi.Sys.Logs;

public interface ILogController: IHasLog
{
    /// <summary>
    /// Used to be GET System/ExtendedLogging
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    // UI test: Apps management > System Info > activate extended page logging.
    string EnableDebug(int duration = 1);
}
