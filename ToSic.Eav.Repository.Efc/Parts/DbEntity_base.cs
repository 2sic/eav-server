using System.Collections.Generic;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity: BllCommandBase
    {
        public DbEntity(DbDataController cntx) : base(cntx)
        {
        }

        public List<LogItem> Log => DbContext.Log;

    }
}
