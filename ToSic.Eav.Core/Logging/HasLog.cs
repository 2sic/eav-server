using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{
    public class HasLog : IHasLog
    {
        protected Log Log { get; private set; }


        public HasLog(string name, Log parentLog = null, string initialMessage = null)
        {
            InitLog(name, parentLog, initialMessage);
        }

        private void InitLog(string name, Log parentLog = null, string initialMessage = null)
        {
            Log = new Log(name, parentLog, initialMessage);
        }

        public void LinkLog(Log parentLog) => Log.LinkTo(parentLog);
    }

}
