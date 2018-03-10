using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockZoneMapper: IZoneMapper
    {
        public int GetZoneId(int tennantId) => -1;

        public int GetZoneId(ITennant tennant) => -999;

        public ITennant Tennant(int zoneId) => new MockTennant();

        public List<TempTempCulture> CulturesWithState(int tennantId, int zoneId) => new List<TempTempCulture>();
    }
}
