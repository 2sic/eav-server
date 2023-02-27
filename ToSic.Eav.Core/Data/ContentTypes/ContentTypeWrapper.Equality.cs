using System;
using System.Net.Mime;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data
{
    public  partial class ContentTypeWrapper : IEquatable<ContentTypeWrapper>, IEquatable<IMultiWrapper<IContentType>>
    {
        public IContentType RootContentsForEqualityCheck => _rootContentsForEqualityCheck.Get(() =>
        {
            var contents = GetContents();
            return contents is IMultiWrapper<IContentType> wrapper
                ? wrapper.RootContentsForEqualityCheck ?? RootContentsForEqualityCheck
                : contents;
        });
        private readonly GetOnce<IContentType> _rootContentsForEqualityCheck = new GetOnce<IContentType>();

        public static bool operator ==(ContentTypeWrapper d1, ContentTypeWrapper d2) => WrapperEquality.IsEqual(d1, d2);

        public static bool operator !=(ContentTypeWrapper d1, ContentTypeWrapper d2) => !WrapperEquality.IsEqual(d1, d2);

        public static bool operator ==(ContentTypeWrapper d1, IMultiWrapper<IContentType> d2) => WrapperEquality.IsEqual(d1, d2);

        public static bool operator !=(ContentTypeWrapper d1, IMultiWrapper<IContentType> d2) => !WrapperEquality.IsEqual(d1, d2);


        public bool Equals(ContentTypeWrapper other) => WrapperEquality.EqualsObj(this, other);

        public bool Equals(IMultiWrapper<IContentType> other) => WrapperEquality.IsEqual(this, other);

        public override bool Equals(object other) => WrapperEquality.EqualsObj(this, other);

        public override int GetHashCode() => WrapperEquality.GetHashCode(this);

    }
}
