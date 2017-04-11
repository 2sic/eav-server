namespace ToSic.Eav.Repository.EF4
{
    public class BllCommandBase
    {
        protected DbDataController DbContext { get; private set; }

        internal BllCommandBase(DbDataController dataController)
        {
            DbContext = dataController;
        }

    }
}
