﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Used to get relationships between entities.
	/// </summary>
	public class RelationshipManager: IRelationshipManager
	{
		private readonly IEntity _entity;
	    public readonly IEnumerable<EntityRelationshipItem> AllRelationships;

		/// <summary>
		/// Initializes a new instance of the RelationshipManager class.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="allAllRelationships"></param>
		public RelationshipManager(IEntity entity, IEnumerable<EntityRelationshipItem> allAllRelationships)
		{
			_entity = entity;
			AllRelationships = allAllRelationships;
		}

		/// <summary>
		/// Get all Child Entities
		/// </summary>
		public IEnumerable<IEntity> AllChildren
		{
			get { return AllRelationships.Where(r => r.Parent == _entity).Select(r => r.Child); }
		}

		/// <summary>
		/// Get all Parent Entities
		/// </summary>
		public IEnumerable<IEntity> AllParents
		{
			get { return AllRelationships.Where(r => r.Child == _entity).Select(r => r.Parent); }
		}

		/// <summary>
		/// Get Children of a specified Attribute Name
		/// </summary>
		public IRelatedEntities Children
		{
			get { return new RelatedEntities(_entity.Attributes); }
		}

	}
}