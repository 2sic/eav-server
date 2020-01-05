using ToSic.Eav.Serialization;

namespace ToSic.Eav.WebApi.Helpers
{
    public static class Serializers
    {
        // I must keep the serializer so it can be configured from outside if necessary
        public static Serializer GetSerializerWithGuidEnabled()
        {
            var serializer = Factory.Resolve<Serializer>();
            serializer.IncludeGuid = true;
            return serializer;
        }
    }
}
