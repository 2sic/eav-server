using ToSic.Eav.Convert;

namespace ToSic.Eav.WebApi.Helpers
{
    public static class Serializers
    {
        // I must keep the serializer so it can be configured from outside if necessary
        public static IConvertToJsonBasic EnableGuids(this IConvertToJsonBasic etd)
        {
            etd.WithGuid = true;
            return etd;
        }
    }
}
