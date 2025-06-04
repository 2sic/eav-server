using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Metadata.Sys;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json;

/// <summary>
/// Controls how serialization should happen, as different scenarios require different parts to be included or not.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class JsonDeSerializationSettings
{
    public IHasMetadataSourceAndExpiring MetadataSource { get; set; }

    public IDeferredContentTypeProvider ContentTypeProvider { get; set; }

    /// <summary>
    /// Experimental WIP 2024-09-30 2dm
    /// Trying to ensure that sub-entities of content-types - such as CSV data sources of the metadata, are shared between all content-types.
    ///
    /// ATM the infrastructure doesn't seem to want to give the appState to the deserializer,
    /// and since we're not sure if we would break something, we're trying to share it like this...
    /// </summary>
    public IEntitiesSource RelationshipsSource { get; set; }
}