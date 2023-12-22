// ReSharper disable once CheckNamespace

using ToSic.Eav.Apps;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Metadata;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json;

/// <summary>
/// Controls how serialization should happen, as different scenarios require different parts to be included or not.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class JsonDeSerializationSettings
{
    public JsonDeSerializationSettings() { }

    public IHasMetadataSource CtAttributeMetadataAppState { get; set; }

    public IDeferredContentTypeProvider EntityContentTypeProvider { get; set; }
}