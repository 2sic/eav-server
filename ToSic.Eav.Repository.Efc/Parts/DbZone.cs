using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbZone: BllCommandBase
    {
        public DbZone(DbDataController cntx) : base(cntx) {}
       
        /// <summary>
        /// Creates a new Zone with a default App and Culture-Root-Dimension
        /// </summary>
        public int AddZone(string name)
        {
            var newZone = new ToSicEavZones { Name = name };
            DbContext.SqlDb.Add(newZone);

            DbContext.Dimensions.AddRootDimension(Constants.CultureSystemKey, "Culture Root", newZone);

            DbContext.App.AddApp(newZone);

            return newZone.ZoneId;
        }

    }
}
