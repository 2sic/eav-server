using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader: IHasLog
    {

        private TimeSpan _sqlTotalTime = new TimeSpan(0);


        private Log Log { get; set; }


        #region IHasLog interface
        public void LinkLog(Log parentLog) => Log.LinkTo(parentLog);
        #endregion
    }
}
