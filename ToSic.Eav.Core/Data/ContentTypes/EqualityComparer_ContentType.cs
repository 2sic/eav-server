namespace ToSic.Eav.Data;

// TODO: V11 move to ContentType itself
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EqualityComparer_ContentType: IEqualityComparer<IContentType>
{
    public bool Equals(IContentType x, IContentType y) 
        => x != null && x.NameId == y?.NameId;

    public int GetHashCode(IContentType obj) 
        => obj.NameId.GetHashCode();
}