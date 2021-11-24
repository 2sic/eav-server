using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonType
    {
        public string Name;
        public string Id;

        /// <summary>
        /// Empty constructor is important for de-serializing
        /// </summary>
        public JsonType() { }

        public JsonType(IEntity entity)
        {
            Name = entity.Type.Name;
            Id = entity.Type.StaticName;
        }
    }
}
