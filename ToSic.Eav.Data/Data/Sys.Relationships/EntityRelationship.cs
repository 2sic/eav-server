namespace ToSic.Eav.Data.Sys.Relationships;

/// <summary>
/// Represents a Relation between two entities, connecting a parent to a child.
/// </summary>
/// <remarks>
/// Initializes a new instance of the EntityRelationshipItem class.
/// </remarks>
/// <param name="Parent">Parent Entity which has a reference to the child</param>
/// <param name="Child">Child Entity which is referenced by the parent</param>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record EntityRelationship(IEntity Parent, IEntity Child) : IEntityRelationship;