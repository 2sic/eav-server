using ToSic.Eav.ImportExport.Json;

namespace ToSic.Eav.ImportExport.Tests.Json
{
    public class JsonTestBase : Eav.Persistence.Efc.Tests.Efc11TestBase
    {
        protected string GetJsonOfEntity(int appId, int eId, JsonSerializer ser = null)
        {
            var exBuilder = ser ?? SerializerOfApp(appId);
            var xmlEnt = exBuilder.Serialize(eId);
            return xmlEnt;
        }
    }
}
