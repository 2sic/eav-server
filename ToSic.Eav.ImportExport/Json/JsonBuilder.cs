using Newtonsoft.Json;

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
