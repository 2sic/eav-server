namespace ToSic.Eav.Model;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract record ModelOfEntityWithLog: ModelOfEntity, IHasLog
{
    /// <summary>
    /// Primary constructor
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="parentLog"></param>
    /// <param name="logName"></param>
    protected ModelOfEntityWithLog(IEntity entity, ILog? parentLog, string logName) : base(entity)
        => Log = new Log(logName, parentLog);

    protected readonly ILog Log = null!;
    ILog IHasLog.Log => Log;
}
