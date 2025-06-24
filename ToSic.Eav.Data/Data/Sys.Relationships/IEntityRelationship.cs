namespace ToSic.Eav.Data.Relationships.Sys;

public interface IEntityRelationship
{
    /// <summary>
    /// The parent item, which has a reference to the child
    /// </summary>
    IEntity Parent { get; }

    /// <summary>
    /// The child item, which is referenced by the parent
    /// </summary>
    IEntity Child { get; }
}