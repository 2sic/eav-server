using System;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader: IHasLog
    {
        private TimeSpan _sqlTotalTime = new TimeSpan(0);
    }
}
