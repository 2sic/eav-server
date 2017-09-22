using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity: BllCommandBase
    {
        public DbEntity(DbDataController cntx, Log parentLog = null) : base(cntx, parentLog, "DbEnty")
        {
        }
    }
}
