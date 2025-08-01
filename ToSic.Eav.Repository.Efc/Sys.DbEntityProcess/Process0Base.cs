namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal abstract class Process0Base(string logName): HelperBase(null, logName), IEntityProcess
{
    /// <summary>
    /// Process one method - the default to implement.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public abstract EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data);

    /// <summary>
    /// Process all - often not implemented again; unless there are certain optimizations.
    /// </summary>
    /// <returns></returns>
    public virtual ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
    {
        var l = GetLogCall(services, logProcess);

        var result = data
            .Select(d => ProcessOne(services, d))
            .ToListOpt();

        return l.Return(result);
    }

    protected ILogCall<ICollection<EntityProcessData>>? GetLogCall(EntityProcessServices services, bool logProcess)
    {
        var log = logProcess ? services.LogSummary : services.LogDetails;
        return log.Fn<ICollection<EntityProcessData>>(GetType().Name, timer: true);
    }
}
