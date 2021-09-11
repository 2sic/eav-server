using ToSic.Eav.Convert;
using ToSic.Eav.ImportExport.JsonLight;

namespace ToSic.Eav.WebApi.Helpers
{
    public static class Serializers
    {
        // I must keep the serializer so it can be configured from outside if necessary
        public static IConvertToJsonLight EnableGuids(this IConvertToJsonLight etd)
        {
            etd.WithGuid = true;
            return etd;
        }
    }
}
