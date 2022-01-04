using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Data
{
    public partial class ContentTypeWrapper: Wrapper<IContentType>, IContentType, IHasDecorators<IContentType>, IMultiWrapper<IContentType>
    {
        public List<IDecorator<IContentType>> Decorators { get; } = new List<IDecorator<IContentType>>();

        /// <summary>
        /// Create a new wrapper.
        /// In case we're re-wrapping another wrapper, make sure we use the real, underlying contentType for the Contents
        /// </summary>
        /// <param name="contentType"></param>
        public ContentTypeWrapper(IContentType contentType) : base((contentType as ContentTypeWrapper)?._contents ?? contentType)
        {
            RootContentsForEqualityCheck = contentType;
            if (contentType is IMultiWrapper<IContentType> wrapper)
                RootContentsForEqualityCheck = wrapper.RootContentsForEqualityCheck ?? RootContentsForEqualityCheck;

            if(contentType is IHasDecorators<IContentType> hasDecorators)
                Decorators.AddRange(hasDecorators.Decorators);
        }

        public ContentTypeWrapper(IContentType contentType, IDecorator<IContentType> decorator) : this(contentType) 
            => Decorators.Add(decorator);

        public int AppId => _contents.AppId;

        public string Name => _contents.Name;

        public string StaticName => _contents.StaticName;

        public string Description => _contents.Description;

        public string Scope => _contents.Scope;

        public int ContentTypeId => _contents.ContentTypeId;

        public IList<IContentTypeAttribute> Attributes
        {
            get => _contents.Attributes;
            set => _contents.Attributes = value;
        }

        public IContentTypeAttribute this[string fieldName] => _contents[fieldName];

        public RepositoryTypes RepositoryType => _contents.RepositoryType;

        public string RepositoryAddress => _contents.RepositoryAddress;

        public bool IsDynamic => _contents.IsDynamic;

        public ContentTypeMetadata Metadata => _contents.Metadata;

        public bool Is(string name) => _contents.Is(name);

        public string DynamicChildrenField => _contents.DynamicChildrenField;

        public bool AlwaysShareConfiguration => _contents.AlwaysShareConfiguration;

        IMetadataOf IHasMetadata.Metadata => ((IHasMetadata)_contents).Metadata;
    }
}
