using System.Collections.Immutable;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using ToSic.Lib.Data;
using ToSic.Lib.Helpers;
using ToSic.Lib.Wrappers;
using ToSic.Sys.Caching.PiggyBack;

namespace ToSic.Eav.Data.ContentTypes.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class ContentTypeWrapper: WrapperLazy<IContentType>, IContentType, IHasDecorators<IContentType>, IMultiWrapper<IContentType>
{
    public ContentTypeWrapper(IContentType contentType) : base((contentType as ContentTypeWrapper)?.GetContents() ?? contentType)
    { }

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


    public IEnumerable<IDecorator<IContentType>> Decorators => _decorators.Get(() =>
    {
        var list = new List<IDecorator<IContentType>>();
        if (_wrapperDecorator != null)
            list.Add(_wrapperDecorator);
        if (GetContents() is IHasDecorators<IContentType> hasDecors)
            list.AddRange(hasDecors.Decorators);
        return list.ToImmutableList();
    });
    private readonly GetOnce<IImmutableList<IDecorator<IContentType>>> _decorators = new();
    private readonly IDecorator<IContentType> _wrapperDecorator;



    public int AppId => GetContents().AppId;

    public string Name => GetContents().Name;

    [Obsolete("Deprecated in v13, please use NameId instead")]
    public string StaticName => GetContents().NameId;

    public string NameId => GetContents().NameId;

    public string Scope => GetContents().Scope;

    public int Id => GetContents().Id;

    [Obsolete("Deprecated in V13, please use Id instead.")]
    public int ContentTypeId => GetContents().Id;

    public IEnumerable<IContentTypeAttribute> Attributes => GetContents().Attributes;

    public IContentTypeAttribute this[string fieldName] => GetContents()[fieldName];

    public RepositoryTypes RepositoryType => GetContents().RepositoryType;

    public string RepositoryAddress => GetContents().RepositoryAddress;

    public bool IsDynamic => GetContents().IsDynamic;

    public IMetadataOf Metadata => GetContents().Metadata;

    public bool Is(string name) => GetContents().Is(name);


    public string TitleFieldName => GetContents().TitleFieldName;

    [PrivateApi] // #SharedFieldDefinition
    public ContentTypeSysSettings SysSettings => GetContents().SysSettings;

    public string DynamicChildrenField => GetContents().DynamicChildrenField;

    public bool AlwaysShareConfiguration => GetContents().AlwaysShareConfiguration;

    IMetadataOf IHasMetadata.Metadata => ((IHasMetadata)GetContents()).Metadata;

    public PiggyBack PiggyBack => GetContents().PiggyBack;
}