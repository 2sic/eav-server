using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Text based identity for debugging only
        /// </summary>
        public static string LogState(this IAppIdentity withAppIdentity) 
            => withAppIdentity.ZoneId + ":" + withAppIdentity.AppId;
    }
}
