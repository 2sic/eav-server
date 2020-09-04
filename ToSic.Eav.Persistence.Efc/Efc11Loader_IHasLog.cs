using System;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader: IHasLog
    {
        private TimeSpan _sqlTotalTime = new TimeSpan(0);

        public ILog Log { get; private set; }
    }
}
