using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.Apps
{
    public class AppRuntime : AppBase
    {
        public AppRuntime(int zoneId, int appId) : base(zoneId, appId) { }

        public AppRuntime(IApp app) : base(app) { }


        public IEnumerable<IContentType> ContentTypes => Cache.GetContentTypes().Select(c => c.Value);

        public IEnumerable<IContentType> GetContentTypes(string scope = null, bool includeAttributeTypes = false)
        {
            //var contentTypes = Cache.GetContentTypes();
            var set = ContentTypes // contentTypes.Select(c => c.Value)
                .Where(c => includeAttributeTypes || !c.Name.StartsWith("@"));
            if (scope != null)
                set = set.Where(p => p.Scope == scope);
            return set.OrderBy(c => c.Name);
        }
    }
}
