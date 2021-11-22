using System;

namespace ToSic.Eav.Data
{
    public  partial class ContentTypeWrapper : IEquatable<ContentTypeWrapper>
    {
        public bool Equals(ContentTypeWrapper other)
        {
            if (ReferenceEquals(null, other?.GetContents())) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(GetContents(), other.GetContents());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContentTypeWrapper)obj);
        }

        public override int GetHashCode() => (GetContents() != null ? GetContents().GetHashCode() : 0);
    }
}
