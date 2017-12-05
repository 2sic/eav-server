using System.Collections.Generic;
using ToSic.Eav.Apps.Environment;

namespace ToSic.Eav.Apps.Interfaces
{
    public interface IZoneMapper<T> : IZoneMapper
    {
        int GetZoneId(Tennant<T> tennant);

        List<TempTempCulture> CulturesWithState(T tennant, int zoneId);
    }
}
