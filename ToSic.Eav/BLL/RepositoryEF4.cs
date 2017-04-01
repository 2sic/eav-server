using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.BLL
{
    /// <summary>
    /// this is a special temporary class to provide certain functions which
    /// the repository should provide, but trying to keep everything else private
    /// so it's a temporary solution, till a new repo is ready
    /// </summary>
    public class RepositoryEF4: BllCommandBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataController"></param>
        public RepositoryEF4(EavDataController dataController) : base(dataController)
        {
        }

        public IEnumerable<dynamic> AppsGet(int zoneId) 
            => Context.App.GetApps().Select(a => new { Id = a.AppID, Name = a.Name, Zone = a.ZoneID });

        public dynamic AppGet(int zoneId, int appId)
        {
            var a = Context.App.GetApps().First(x => x.AppID == appId);
            return new { Id = a.AppID, Name = a.Name, Zone = a.ZoneID };
        }

    }
}
