using ToSic.Lib.Helpers;
using static ToSic.Eav.Data.MultiWrapperEquality;

namespace ToSic.Eav.Data.ContentTypes.Sys;

partial class ContentTypeWrapper : IEquatable<ContentTypeWrapper>, IEquatable<IMultiWrapper<IContentType>>
{
    public IContentType RootContentsForEqualityCheck => _rootContentsForEqualityCheck.Get(() =>
    {
        var contents = GetContents();
        return contents is IMultiWrapper<IContentType> wrapper
            ? wrapper.RootContentsForEqualityCheck ?? RootContentsForEqualityCheck
            : contents;
    });
    private readonly GetOnce<IContentType> _rootContentsForEqualityCheck = new();

    public static bool operator ==(ContentTypeWrapper d1, ContentTypeWrapper d2) => IsEqual(d1, d2);

    public static bool operator !=(ContentTypeWrapper d1, ContentTypeWrapper d2) => !IsEqual(d1, d2);

    public static bool operator ==(ContentTypeWrapper d1, IMultiWrapper<IContentType> d2) => IsEqual(d1, d2);

    public static bool operator !=(ContentTypeWrapper d1, IMultiWrapper<IContentType> d2) => !IsEqual(d1, d2);


    public bool Equals(ContentTypeWrapper other) => EqualsObj(this, other);

    public bool Equals(IMultiWrapper<IContentType> other) => IsEqual(this, other);

    public override bool Equals(object other) => EqualsObj(this, other);

    public override int GetHashCode() => GetWrappedHashCode(this);

}