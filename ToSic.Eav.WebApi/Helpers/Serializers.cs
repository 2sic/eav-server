using ToSic.Eav.Conversion;

namespace ToSic.Eav.WebApi.Helpers
{
    public static class Serializers
    {
        // I must keep the serializer so it can be configured from outside if necessary
        public static EntitiesToDictionary EnableGuids(this EntitiesToDictionary etd)
        {
            etd.WithGuid = true;
            return etd;
        }
    }
}
