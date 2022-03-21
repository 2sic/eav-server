using System;
using Newtonsoft.Json;

namespace ToSic.Eav.Plumbing
{
    // Inspired by https://stackoverflow.com/questions/5780888/casting-interfaces-for-deserialization-in-json-net
    // note that it quickly fails, if the object has a sub-object of the same interface (stack overflow)
    // so best not use yet, until you know what you're doing 
    // see also https://stackoverflow.com/questions/22407321/stackoverflowexception-in-my-jsonconverter-class-when-using-jsonconverter-attr
    public class ConcreteTypeConverter<TClass, TInterface> : JsonConverter where TClass : TInterface
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(TInterface);

        public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer jser)
            => jser.Deserialize<TClass>(reader);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer jser)
            => jser.Serialize(writer, value);
    }
}
