namespace ToSic.Eav.BLL
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
