namespace ToSic.Eav.Logging
{
    public abstract class HasLog<T> : HasLog, IHasLog<T> where T: class
    {
        protected HasLog(string logName, ILog parentLog = null, string initialMessage = null, string cPath = null, string cName = null, int cLine = 0) 
            : base(logName, parentLog, initialMessage, cPath, cName, cLine)
        {
        }

        protected HasLog(string logName, CodeRef code, ILog parentLog = null, string initialMessage = null) 
            : base(logName, code, parentLog, initialMessage)
        {
        }

        public T Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this as T;
        }

        public T Init(ILog parent, string logName)
        {
            Log.LinkTo(parent, logName);
            return this as T;
        }

    }
}
