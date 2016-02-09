namespace ToSic.Eav.BLL
{
    public class BllCommandBase
    {
        public EavDataController Context { get; internal set; }

        public BllCommandBase(EavDataController dataController)
        {
            Context = dataController;
        }

    }
}
