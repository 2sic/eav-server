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
    /// <param name="services"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public virtual ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data)
    {
        var result = data
            .Select(d => ProcessOne(services, d))
            .ToListOpt();
        return result;
    }

}
