namespace ToSic.Eav.Data.Sys.Entities;

public abstract record RecordOfEntityWithLog: RecordOfEntityWithIds, IHasLog
{
    /// <summary>
    /// Primary constructor
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="parentLog"></param>
    /// <param name="logName"></param>
    protected RecordOfEntityWithLog(IEntity entity, ILog? parentLog, string logName) : base(entity)
        => Log = new Log(logName, parentLog);

    protected readonly ILog Log = null!;
    ILog IHasLog.Log => Log;
}
