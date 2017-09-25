using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbZone: BllCommandBase
    {
        public DbZone(DbDataController cntx) : base(cntx, "DbZone") {}
       
        /// <summary>
        /// Creates a new Zone with a default App and Culture-Root-Dimension
        /// </summary>
        public int AddZone(string name)
        {
            var newZone = new ToSicEavZones { Name = name };
            DbContext.SqlDb.Add(newZone);

            DbContext.Dimensions.AddRootCultureNode(Constants.CultureSystemKey, "Culture Root", newZone);

            DbContext.App.AddApp(newZone);

            return newZone.ZoneId;
        }

    }
}
