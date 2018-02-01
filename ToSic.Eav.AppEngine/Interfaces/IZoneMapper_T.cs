using System.Collections.Generic;
using ToSic.Eav.Apps.Environment;

namespace ToSic.Eav.Apps.Interfaces
{
    public interface IZoneMapper<T> : IZoneMapper
    {
        int GetZoneId(ITennant tennant);

        //List<TempTempCulture> CulturesWithState(ITennant tennant, int zoneId);
    }
}
