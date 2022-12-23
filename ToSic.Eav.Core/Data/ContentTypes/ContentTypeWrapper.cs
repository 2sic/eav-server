using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using ToSic.Lib.Data;

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
        public ContentTypeWrapper(IContentType contentType) : base((contentType as ContentTypeWrapper)?.UnwrappedContents ?? contentType)
        {
            RootContentsForEqualityCheck = contentType;
            if (contentType is IMultiWrapper<IContentType> wrapper)
                RootContentsForEqualityCheck = wrapper.RootContentsForEqualityCheck ?? RootContentsForEqualityCheck;

            if(contentType is IHasDecorators<IContentType> hasDecorators)
                Decorators.AddRange(hasDecorators.Decorators);
        }

        public ContentTypeWrapper(IContentType contentType, IDecorator<IContentType> decorator) : this(contentType) 
            => Decorators.Add(decorator);

        public int AppId => UnwrappedContents.AppId;

        public string Name => UnwrappedContents.Name;

        [Obsolete("Deprecated in v13, please use NameId instead")]
        public string StaticName => UnwrappedContents.NameId;

        public string NameId => UnwrappedContents.NameId;

        // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
        //public string Description => _contents.Description;

        public string Scope => UnwrappedContents.Scope;

        public int Id => UnwrappedContents.Id;

        [Obsolete("Deprecated in V13, please use Id instead.")]
        public int ContentTypeId => UnwrappedContents.Id;

        public IList<IContentTypeAttribute> Attributes
        {
            get => UnwrappedContents.Attributes;
            set => UnwrappedContents.Attributes = value;
        }

        public IContentTypeAttribute this[string fieldName] => UnwrappedContents[fieldName];

        public RepositoryTypes RepositoryType => UnwrappedContents.RepositoryType;

        public string RepositoryAddress => UnwrappedContents.RepositoryAddress;

        public bool IsDynamic => UnwrappedContents.IsDynamic;

        public ContentTypeMetadata Metadata => UnwrappedContents.Metadata;

        public bool Is(string name) => UnwrappedContents.Is(name);

        public string DynamicChildrenField => UnwrappedContents.DynamicChildrenField;

        public bool AlwaysShareConfiguration => UnwrappedContents.AlwaysShareConfiguration;

        IMetadataOf IHasMetadata.Metadata => ((IHasMetadata)UnwrappedContents).Metadata;
    }
}
