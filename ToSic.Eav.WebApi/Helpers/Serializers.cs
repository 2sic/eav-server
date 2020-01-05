using ToSic.Eav.Serialization;

namespace ToSic.Eav.WebApi.Helpers
{
    public static class Serializers
    {
        // I must keep the serializer so it can be configured from outside if necessary
        public static EntitiesToDictionary GetSerializerWithGuidEnabled()
        {
            var serializer = Factory.Resolve<EntitiesToDictionary>();
            serializer.WithGuid = true;
            return serializer;
        }
    }
}
