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
        public ContentTypeWrapper(IContentType contentType) : base((contentType as ContentTypeWrapper)?.GetContents() ?? contentType)
        {
            RootContentsForEqualityCheck = contentType;
            if (contentType is IMultiWrapper<IContentType> wrapper)
                RootContentsForEqualityCheck = wrapper.RootContentsForEqualityCheck ?? RootContentsForEqualityCheck;

            if(contentType is IHasDecorators<IContentType> hasDecorators)
                Decorators.AddRange(hasDecorators.Decorators);
        }

        public ContentTypeWrapper(IContentType contentType, IDecorator<IContentType> decorator) : this(contentType) 
            => Decorators.Add(decorator);

        public int AppId => GetContents().AppId;

        public string Name => GetContents().Name;

        [Obsolete("Deprecated in v13, please use NameId instead")]
        public string StaticName => GetContents().NameId;

        public string NameId => GetContents().NameId;

        // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
        //public string Description => _contents.Description;

        public string Scope => GetContents().Scope;

        public int Id => GetContents().Id;

        [Obsolete("Deprecated in V13, please use Id instead.")]
        public int ContentTypeId => GetContents().Id;

        public IList<IContentTypeAttribute> Attributes => GetContents().Attributes;

        public IContentTypeAttribute this[string fieldName] => GetContents()[fieldName];

        public RepositoryTypes RepositoryType => GetContents().RepositoryType;

        public string RepositoryAddress => GetContents().RepositoryAddress;

        public bool IsDynamic => GetContents().IsDynamic;

        public ContentTypeMetadata Metadata => GetContents().Metadata;

        public bool Is(string name) => GetContents().Is(name);


        public string TitleFieldName => GetContents().TitleFieldName;

        public string DynamicChildrenField => GetContents().DynamicChildrenField;

        public bool AlwaysShareConfiguration => GetContents().AlwaysShareConfiguration;

        IMetadataOf IHasMetadata.Metadata => ((IHasMetadata)GetContents()).Metadata;
    }
}
