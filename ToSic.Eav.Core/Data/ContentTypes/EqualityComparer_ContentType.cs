using System.Collections.Generic;

namespace ToSic.Eav.Data
{
    // TODO: V11 move to ContentType itself
    public class EqualityComparer_ContentType: IEqualityComparer<IContentType>
    {
        public bool Equals(IContentType x, IContentType y) 
            => x != null && x.StaticName == y?.StaticName;

        public int GetHashCode(IContentType obj) 
            => obj.StaticName.GetHashCode();
    }
}
