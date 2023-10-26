// ReSharper disable once CheckNamespace

using ToSic.Eav.Apps;
using ToSic.Eav.Data.ContentTypes;

namespace ToSic.Eav.ImportExport.Json
{
    /// <summary>
    /// Controls how serialization should happen, as different scenarios require different parts to be included or not.
    /// </summary>
    public class JsonDeSerializationSettings
    {
        public JsonDeSerializationSettings() { }

        public AppState CtAttributeMetadataAppState { get; set; }

        public IDeferredContentTypeProvider EntityContentTypeProvider { get; set; }
    }
}
