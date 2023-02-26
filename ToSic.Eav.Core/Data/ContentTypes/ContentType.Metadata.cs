using System;
using ToSic.Eav.Metadata;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data
{
    public partial class ContentType
    {

        #region Metadata

        /// <inheritdoc />
        public ContentTypeMetadata Metadata { get; }
        //public ContentTypeMetadata Metadata => _metadata.Get(() => new ContentTypeMetadata(NameId, _metaSourceFinder, Name));
        //private readonly GetOnce<ContentTypeMetadata> _metadata = new GetOnce<ContentTypeMetadata>();
        //private readonly Func<IHasMetadataSource> _metaSourceFinder;

        IMetadataOf IHasMetadata.Metadata => Metadata;

        #endregion
    }
}
