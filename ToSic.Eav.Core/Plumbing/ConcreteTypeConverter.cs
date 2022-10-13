using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToSic.Eav.Plumbing
{
    // Inspired by https://stackoverflow.com/questions/5780888/casting-interfaces-for-deserialization-in-json-net
    // note that it quickly fails, if the object has a sub-object of the same interface (stack overflow)
    // so best not use yet, until you know what you're doing 
    // see also https://stackoverflow.com/questions/22407321/stackoverflowexception-in-my-jsonconverter-class-when-using-jsonconverter-attr
    public class ConcreteTypeConverter<TClass, TInterface> : JsonConverter<TClass> where TClass : TInterface
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(TInterface) || typeof(TInterface).IsAssignableFrom(objectType);

        public override TClass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<TClass>(ref reader, options);

        public override void Write(Utf8JsonWriter writer, TClass value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, options);
    }
}
