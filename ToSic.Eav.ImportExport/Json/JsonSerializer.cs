using System;
using Newtonsoft.Json;
using ToSic.Eav.Persistence.Xml;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: SerializerBase
    {
        public const string ReadOnlyMarker = "~";
        public const string NoLanguage = "*";


        private static JsonSerializerSettings JsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            return settings;
        }




 
    }

    internal static class StringHelpers
    {
        public static string EmptyAlternative(this string s, string alternative) => String.IsNullOrEmpty(s) ? alternative : s;
    }
}
