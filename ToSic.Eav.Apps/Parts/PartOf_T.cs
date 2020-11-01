using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Base class for any kind of read/runtime operations
    /// </summary>
    public abstract class PartOf<TParent, TInit> : HasLog where TInit: PartOf<TParent, TInit>
    {
        // ReSharper disable once InconsistentNaming
        protected TParent Parent;

        protected PartOf(string logName): base(logName) { }

        public TInit Init(TParent appRt, ILog parentLog, string logName = null)
        {
            Log.LinkTo(parentLog, logName);
            Parent = appRt;
            return this as TInit;
        }
        

    }
}
