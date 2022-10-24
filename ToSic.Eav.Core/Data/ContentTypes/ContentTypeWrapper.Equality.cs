using System;
using ToSic.Eav.Data.Shared;

namespace ToSic.Eav.Data
{
    public  partial class ContentTypeWrapper : IEquatable<ContentTypeWrapper>, IEquatable<IMultiWrapper<IContentType>>
    {
        public IContentType RootContentsForEqualityCheck { get; }

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
