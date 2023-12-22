namespace ToSic.Eav.Data.PropertyLookup;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class StackAddress
{
    internal StackAddress(IPropertyStackLookup source, string field, int index, StackAddress ancestor)
    {
        Source = source;
        Field = field;
        Index = index;
        Ancestor = ancestor;
    }

    /// <summary>
    /// The parent LookupStack which would get another source in case this doesn't find something
    /// </summary>
    public readonly IPropertyStackLookup Source;

    /// <summary>
    /// The name in the parent, which resulted in this object being created. 
    /// </summary>
    public readonly string Field;

    public readonly int Index;

    public readonly StackAddress Ancestor;

    public StackAddress NewWithOtherIndex(int index) => new(Source, Field, index, Ancestor);

    public StackAddress Child(IPropertyStackLookup source, string field, int index) => new(source, field, index, this);
}