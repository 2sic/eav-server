using Newtonsoft.Json;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Json
{
    public class JsonBuilder
    {
        public JsonBuilder()
        {
            
        }

        public string Serialize(IEntity entity)
        {
            var simple = JsonConvert.SerializeObject(entity);
            return simple;
        }
    }
}
