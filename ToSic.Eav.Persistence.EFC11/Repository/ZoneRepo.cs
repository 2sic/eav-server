using System;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Persistence.EFC11.Repository
{
    public class ZoneRepo: EfcRepoPart
    {

        public ZoneRepo(EfcRepository parent) : base(parent) { }

        // todo: not ready yet!!!
        public static int Create(string name)
        {
            throw new Exception("not ready - must add dims etc.");
            var ctx = Factory.Resolve<EavDbContext>();
            var newZone = new ToSicEavZones { Name = name };
            ctx.ToSicEavZones.Add(newZone);
            ctx.SaveChanges();
            return newZone.ZoneId;
        }

        /// <summary>
        /// Creates a new Zone with a default App and Culture-Root-Dimension
        /// </summary>
        public Tuple<ToSicEavZones, ToSicEavApps> AddZone(string name)
        {
            var newZone = new ToSicEavZones { Name = name };
            Db.ToSicEavZones.Add(newZone);

            Parent.Dimensions.AddDimension(Constants.CultureSystemKey, "Culture Root", newZone);

            var newApp = Db.ToSicEavApps.Add(newZone);

            Db.SaveChanges();

            return Tuple.Create(newZone, newApp);
        }
    }
}
