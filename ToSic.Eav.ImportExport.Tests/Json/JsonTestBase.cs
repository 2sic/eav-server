using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.ImportExport.Json;

namespace ToSic.Eav.ImportExport.Tests.Json
{
    public class JsonTestBase : Persistence.Efc.Tests.Efc11TestBase
    {

        protected string GetJsonOfEntity(int appId, int eId, JsonSerializer ser = null)
        {
            var exBuilder = ser ?? SerializerOfApp(appId);
            var xmlEnt = exBuilder.Serialize(eId);
            return xmlEnt;
        }
    }
}
