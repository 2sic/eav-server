using ToSic.Eav.Apps;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.ImportExport.Serialization
{
    public static class SerializerExtensions
    {
        public static T SetApp<T>(this T serializer, AppState package) where T : SerializerBase
        {
            serializer.Initialize(package);
            return serializer;
        }
    }
}