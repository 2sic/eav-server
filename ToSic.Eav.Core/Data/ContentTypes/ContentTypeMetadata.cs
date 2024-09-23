using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data;

/// <summary>
/// Provides metadata for a content type. This can be very special, because ContentTypes can be shared (ghosts),
/// in which case the metadata must be retrieved from another "remote" location (where the original is defined). 
/// </summary>
[PrivateApi("2021-09-30 hidden now, previously InternalApi_DoNotUse_MayChangeWithoutNotice this is just fyi")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ContentTypeMetadata : MetadataOf<string>
{
    /// <summary>
    /// Used in cases where the metadata-provider is already known
    /// </summary>
    /// <param name="typeId">type id / static-name</param>
    /// <param name="items"></param>
    /// <param name="deferredSource">remote / deferred metadata provider</param>
    /// <param name="title"></param>
    internal ContentTypeMetadata(string typeId, List<IEntity> items, Func<IHasMetadataSourceAndExpiring> deferredSource, string title)
        : base(targetType: (int)TargetTypes.ContentType, key: typeId, title: title, items: items, deferredSource: deferredSource)
    {
    }

    /// <summary>
    /// Description <see cref="IEntity"/> metadata of this content-type.
    /// </summary>
    [PrivateApi("was public in the class which used to be public, so it may be used, but try to privatise as we don't plan to publish this")]
    public IEntity Description => this.FirstOrDefaultOfType(ContentTypeDetails.ContentTypeTypeName);

    public ContentTypeDetails DetailsOrNull
    {
        get
        {
            var desc = Description;
            return desc == null ? null : new ContentTypeDetails(desc);
        }
    }

    /// <summary>
    /// Load / initialize - needed when building the cache.
    /// Must usually be called a bit later, because the data is initialized from a cache, which in case of ghosts may be loaded a bit later.
    /// </summary>
    protected override List<IEntity> LoadFromProviderInsideLock(IList<IEntity> additions = default)
    {
        // add the guid metadata on entity if it has a real guid
        // this is kind of wrong, because it should use the type MetadataForContentType
        // but this slipped in a long time ago, and we cannot change it any more
        var hasProperGuid = Guid.TryParse(Key, out var ctGuid);
        var mdUsingGuid = !hasProperGuid
            ? null
            : GetMetadataSource()?
                .GetMetadata(TargetTypes.Entity, ctGuid)
                .ToList();

        // combine with base string based metadata
        return base.LoadFromProviderInsideLock(mdUsingGuid);
    }

}