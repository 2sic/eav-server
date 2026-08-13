using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Metadata;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys;

internal class ContentTypeUtil
{
    private static Guid SafeConvertGuid(IContentType contentType)
        => Guid.TryParse(contentType.NameId, out var guid)
            ? guid
            : Guid.Empty;

    [ContentTypeUse(Type = typeof(ContentType))]
    internal class ContentTypeSummary(IContentType contentType, int items)
        : IRawEntityAutoConvert, IHasMetadata
    {
        public int Id => contentType.Id;
        public Guid Guid => SafeConvertGuid(contentType);

        [ContentTypeIgnore]
        public IMetadata Metadata => contentType.Metadata;

        [ContentTypeTitle]
        public string Name => contentType.Name;

        public string NameId => contentType.NameId;
        public bool IsDynamic => contentType.IsDynamic;
        public string Scope => contentType.Scope;
        public int AttributesCount => contentType.Attributes.Count();
        public int Items => items;
        public string RepositoryType => contentType.RepositoryType.ToString();
        public string RepositoryAddress => contentType.RepositoryAddress;

        // 2024-10-29 v18.03 2dm disabled, as deprecated, must see if something breaks, but don't really expect it...
        // noticed that it's actually used quite a bit in our internal fields, would have to change that first...
        // I must also assume that it may have been used elsewhere too, but I don't really think so...
        public string StaticName => contentType.NameId; // TODO: This should be removed, but JS code still uses it, so it must be changed first
    }
}
