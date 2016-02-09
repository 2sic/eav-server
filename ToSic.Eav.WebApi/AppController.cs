using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for Apps
	/// </summary>
	public class AppController : Eav3WebApiBase
    {
        [HttpGet]
	    public IEnumerable<dynamic> Get(int zoneId)
        {
            var apps = CurrentContext.App.GetApps();
            return apps.Select(a => new {Id = a.AppID, Name = a.Name, Zone = a.ZoneID});
        }

        [HttpGet]
	    public dynamic Get(int zoneId, int appId)
        {
            var a = CurrentContext.App.GetApps().First(x => x.AppID == appId);
            return new {Id = a.AppID, Name = a.Name, Zone = a.ZoneID};
        }

	    [HttpDelete]
	    public bool Delete(int zoneId, int appId)
	    {
            // todo: check if allawed
            throw new NotImplementedException();

        }

	    [HttpPost]
	    public int Create(int zoneId, string name)
	    {
	        throw new NotImplementedException();
	    }

    }
}