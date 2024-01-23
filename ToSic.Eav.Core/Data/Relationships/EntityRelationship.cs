using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents a Relation between two entities, connecting a parent to a child.
/// </summary>
/// <remarks>
/// Initializes a new instance of the EntityRelationshipItem class.
/// </remarks>
/// <param name="parent">Parent Entity</param>
/// <param name="child">Child Entity</param>
[PrivateApi("2021-09-30 hidden now, previously PublicApi_Stable_ForUseInYourCode - should probably create an interface for this")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EntityRelationship(IEntity parent, IEntity child)
{
    /// <summary>
    /// The parent item, which has a reference to the child
    /// </summary>
    public IEntity Parent { get; } = parent;

    /// <summary>
    /// The child item, which is referenced by the parent
    /// </summary>
    public IEntity Child { get; } = child;

    /// <summary>
    /// ToString for better debugging
    /// </summary>
    public override string ToString() => $"{Parent.EntityId} to {Child.EntityId} ({nameof(EntityRelationship)})";
}