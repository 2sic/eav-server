using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Used to get relationships between entities.
	/// </summary>
	public class RelationshipManager: IRelationshipManager
	{
		private readonly IEntityLight _entity;
	    public readonly IEnumerable<EntityRelationshipItem> AllRelationships;

		/// <summary>
		/// Initializes a new instance of the RelationshipManager class.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="allRelationships"></param>
		public RelationshipManager(IEntityLight entity, IEnumerable<EntityRelationshipItem> allRelationships)
		{
			_entity = entity;
			AllRelationships = allRelationships;
		}

		/// <summary>
		/// Get all Child Entities
		/// </summary>
		public IEnumerable<IEntity> AllChildren => AllRelationships.Where(r => r.Parent == _entity).Select(r => r.Child);

	    /// <summary>
		/// Get all Parent Entities
		/// </summary>
		public IEnumerable<IEntity> AllParents => AllRelationships.Where(r => r.Child == _entity).Select(r => r.Parent);

	    /// <summary>
	    /// Get Children of a specified Attribute Name
	    /// </summary>
	    public IRelatedEntities Children 
            => _entity is IEntity ? new RelatedEntities(((IEntity) _entity).Attributes) : null ; //new RelatedEntities(_entity.Attributes);
        // special note: ATM everything is an IEntity, so EntityLight is currently not supported
	}
}