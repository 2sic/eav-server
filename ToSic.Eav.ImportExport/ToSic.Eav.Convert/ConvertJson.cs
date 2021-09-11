using Newtonsoft.Json;
using ToSic.Eav.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Convert
{
    [PrivateApi("Hide implementation")]
    public class ConvertJson: IConvertJson
    {
        /// <inheritdoc />
        public T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);

        /// <inheritdoc />
        public object Deserialize(string json) => JsonConvert.DeserializeObject(json);

        /// <inheritdoc />
        public string Serialize(object dynamicEntity) => JsonConvert.SerializeObject(dynamicEntity);
    }
}
