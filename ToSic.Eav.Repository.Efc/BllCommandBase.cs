namespace ToSic.Eav.Repository.Efc
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
