using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Apps
{
    public class SystemRuntime
    {
        /// <summary>
        /// Retrieve the Assignment-Type-ID which is used to determine which type of object
        /// an entity is assigned to (because just the object ID would be ambiguous)
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static int GetKeyTypeId(string typeName)
            => DataSource.GetCache(null).GetAssignmentObjectTypeId(typeName);


    }
}
