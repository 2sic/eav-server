using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using ToSic.Lib.Data;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data
{
    public partial class ContentTypeWrapper: WrapperLazy<IContentType>, IContentType, IHasDecorators<IContentType>, IMultiWrapper<IContentType>
    {
        public ContentTypeWrapper(IContentType contentType) : base((contentType as ContentTypeWrapper)?.GetContents() ?? contentType)
        {
        }

        public ContentTypeWrapper(IContentType contentType, IDecorator<IContentType> wrapperDecorator = null) : this(contentType)
        {
            _wrapperDecorator = wrapperDecorator;
        }

        public ContentTypeWrapper(Func<IContentType> lazy) : base(lazy)
        {
            IsDeferred = true;
        }

        /// <summary>
        /// Marks this as being a deferred content-type, which could be reloaded at some point
        /// </summary>
        public bool IsDeferred { get; }

        public new void Reset() => base.Reset();


        public List<IDecorator<IContentType>> Decorators => _decorators.Get(() =>
        {
            var list = new List<IDecorator<IContentType>>();
            if (_wrapperDecorator != null) list.Add(_wrapperDecorator);
            if (GetContents() is IHasDecorators<IContentType> hasDecors) list.AddRange(hasDecors.Decorators);
            return list;
        });
        private readonly GetOnce<List<IDecorator<IContentType>>> _decorators = new GetOnce<List<IDecorator<IContentType>>>();
        private readonly IDecorator<IContentType> _wrapperDecorator;



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
