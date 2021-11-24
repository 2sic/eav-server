using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Data
{
    public partial class ContentTypeWrapper: Wrapper<IContentType>, IContentType, IHasDecorators<IContentType>
    {
        public List<IDecorator<IContentType>> Decorators { get; } = new List<IDecorator<IContentType>>();

        /// <summary>
        /// Create a new wrapper.
        /// In case we're re-wrapping another wrapper, make sure we use the real, underlying contentType for the Contents
        /// </summary>
        /// <param name="contentType"></param>
        public ContentTypeWrapper(IContentType contentType) : base((contentType as ContentTypeWrapper)?.GetContents() ?? contentType)
        {
            if (contentType is ContentTypeWrapper wrapper) 
                Decorators.AddRange(wrapper.Decorators);

        }

        public int AppId => GetContents().AppId;

        public string Name => GetContents().Name;

        public string StaticName => GetContents().StaticName;

        public string Description => GetContents().Description;

        public string Scope => GetContents().Scope;

        public int ContentTypeId => GetContents().ContentTypeId;

        public IList<IContentTypeAttribute> Attributes
        {
            get => GetContents().Attributes;
            set => GetContents().Attributes = value;
        }

        public IContentTypeAttribute this[string fieldName] => GetContents()[fieldName];

        public RepositoryTypes RepositoryType => GetContents().RepositoryType;

        public string RepositoryAddress => GetContents().RepositoryAddress;

        public bool IsDynamic => GetContents().IsDynamic;

        public ContentTypeMetadata Metadata => GetContents().Metadata;

        public bool Is(string name)
        {
            return GetContents().Is(name);
        }

        public string DynamicChildrenField => GetContents().DynamicChildrenField;

        IMetadataOf IHasMetadata.Metadata => ((IHasMetadata)GetContents()).Metadata;
    }
}
