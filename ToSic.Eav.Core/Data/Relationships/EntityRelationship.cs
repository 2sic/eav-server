using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents a Relation between two entities, connecting a parent to a child.
/// </summary>
[PrivateApi("2021-09-30 hidden now, previously PublicApi_Stable_ForUseInYourCode - should probably create an interface for this")]
public class EntityRelationship
{
    /// <summary>
    /// Initializes a new instance of the EntityRelationshipItem class.
    /// </summary>
    /// <param name="parent">Parent Entity</param>
    /// <param name="child">Child Entity</param>
    public EntityRelationship(IEntity parent, IEntity child)
    {
        Parent = parent;
        Child = child;
    }

    /// <summary>
    /// The parent item, which has a reference to the child
    /// </summary>
    public IEntity Parent { get; }

    /// <summary>
    /// The child item, which is referenced by the parent
    /// </summary>
    public IEntity Child { get; }
}