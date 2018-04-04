using System;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader: IHasLog
    {

        private TimeSpan _sqlTotalTime = new TimeSpan(0);


        public Log Log { get; private set; }


        #region IHasLog interface
        public void LinkLog(Log parentLog) => Log.LinkTo(parentLog);
        #endregion
    }
}
