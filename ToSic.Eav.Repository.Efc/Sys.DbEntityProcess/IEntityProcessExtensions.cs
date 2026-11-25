namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

internal static class IEntityProcessExtensions
{
    extension(IEntityProcess process)
    {
        internal ILogCall<ICollection<EntityProcessData>>? GetLogCall(EntityProcessServices services, bool logProcess)
        {
            var log = logProcess ? services.LogSummary : services.LogDetails;
            return log.Fn<ICollection<EntityProcessData>>(process.GetType().Name, timer: true);
        }

        /// <summary>
        /// Process all - often not implemented again; unless there are certain optimizations.
        /// </summary>
        /// <returns></returns>
        internal ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess, Func<EntityProcessServices, EntityProcessData, EntityProcessData> processOne)
        {
            var l = process.GetLogCall(services, logProcess);

            var result = data
                .Select(d => processOne(services, d))
                .ToListOpt();

            return l.Return(result);
        }
    }
}