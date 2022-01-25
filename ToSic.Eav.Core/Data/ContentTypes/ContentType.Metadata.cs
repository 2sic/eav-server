using System;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data
{
    public partial class ContentType
    {

        #region Metadata

        /// <inheritdoc />
        public ContentTypeMetadata Metadata => _metadata ?? (_metadata = new ContentTypeMetadata(NameId, _metaSourceFinder, Name));

        private ContentTypeMetadata _metadata;
        private readonly Func<IHasMetadataSource> _metaSourceFinder;

        IMetadataOf IHasMetadata.Metadata => Metadata;

        #endregion
    }
}
