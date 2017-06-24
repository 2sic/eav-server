﻿using System.Collections.Generic;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {
        public DbEntity(DbDataController cntx) : base(cntx)
        {
        }

        public List<ImportLogItem> ImportLog => DbContext.ImportLog;

    }
}
