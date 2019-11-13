using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Represents a Relation between two entities, connecting a parent to a child.
	/// </summary>
	[PublicApi]
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
		public IEntity Parent { get; internal set; }

        /// <summary>
        /// The child item, which is referenced by the parent
        /// </summary>
		public IEntity Child { get; internal set; }
	}
}
