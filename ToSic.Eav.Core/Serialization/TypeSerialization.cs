namespace ToSic.Eav.Serialization;

/// <summary>
/// Specs for serializing the type. ATM only used for Metadata!
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class TypeSerialization
{
    public bool Serialize { get; set; }

    public bool WithDescription { get; set; }
}