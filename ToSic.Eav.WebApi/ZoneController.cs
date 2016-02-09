using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for Zones
	/// </summary>
	public class ZoneController : Eav3WebApiBase
    {
        /// <summary>
        /// Get a list of all zones
        /// </summary>
        /// <returns></returns>
	    public IEnumerable<dynamic> Get()
	    {
	        var zones = CurrentContext.Zone.GetZones().OrderBy(z => z.ZoneID);
	        return zones.Select(z => new {Id = z.ZoneID, z.Name});
	    }

	    public dynamic Get(int zoneId)
	    {
            var z = CurrentContext.Zone.GetZone(zoneId);
	        return new { Id = z.ZoneID, z.Name }; 
	    }

    }
}