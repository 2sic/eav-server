using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Persistence.EFC11.Models;


namespace ToSic.Eav.Persistence.EFC11.Repository
{
    public class EfcRepository
    {
        public ZoneRepo Zone { get; }
        public DimensionsRepo Dimensions { get; }
        public AppRepo App { get; }

        public AttributeSetRepo AttributeSet { get; }

        public EfcRepository(EavDbContext ctx) 
        {
            Db = ctx;
            Zone = new ZoneRepo(this);
            Dimensions = new DimensionsRepo(this);
            App = new AppRepo(this);
            AttributeSet = new AttributeSetRepo(this);
        }

        internal EavDbContext Db { get; }
        internal int ZoneId {
            get
            {
                if (_zoneId != -1) return _zoneId;
                throw new Exception("can't access zone id - it must be set first");
            }
            set
            {
                _zoneId = value;
            }
        }
        private int _zoneId = -1;

        public static EfcRepository Instance(int zoneId)
        {
            var x = Factory.Resolve<EfcRepository>();
            x.ZoneId = zoneId;
            return x;
        }
    }
}
