using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Repository.Interfaces;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Persistence.EFC11.Repo
{
    internal class ZoneManager : IRepoZoneManager
    {
        private EavDbContext _context;
        internal ZoneManager(EavDbContext context)
        {
            _context = context;
        }

        public IRepoZone Create(string name)
        {
            var z = _context.ToSicEavZones.Add(new ToSicEavZones() { Name = name }) as IRepoZone;
            _context.SaveChanges();
            return z;
        }

        public void Delete(int zoneId)
        {
            _context.ToSicEavZones.Remove(new ToSicEavZones() { ZoneId = zoneId });
            _context.SaveChanges();
        }

        public IEnumerable<IRepoZone> GetAll()
        {
            return _context.ToSicEavZones;
        }

        public IRepoZone GetOne(int zoneId)
        {
            return _context.ToSicEavZones.Find(zoneId);
        }


        public void Update(IRepoZone zone, string name)
        {
            throw new NotImplementedException();
            zone.Name = name;
        }
    }
}
