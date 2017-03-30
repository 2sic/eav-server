using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Persistence.EFC11
{
    public class Experiments
    {
        static void something()
        {
            using (var db = new EavDbContext())
            {
                var results = db.ToSicEavZones.Where(x => x.Name == "Default");

            }
        }

    }
}
