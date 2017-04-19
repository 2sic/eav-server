using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Persistence.EFC11.Repository
{
    public class EfcRepoPart
    {
        internal EfcRepository Parent { get; }
        internal virtual EavDbContext Db => Parent.Db;

        // internal virtual int ZoneId => Parent.ZoneId;
        //public EfcRepoPart(EavDbContext ctx)
        //{
        //    Db = ctx;
        //}

        public EfcRepoPart(EfcRepository parent)
        {
            Parent = parent;
        }
    }
}
