using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.App;
using ToSic.Eav.Caching;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Used to get relationships between entities.
	/// </summary>
	public class RelationshipManager: IRelationshipManager
	{
        // special note: ATM everything is an IEntity, so EntityLight is currently not supported

        private readonly IEntityLight _entity;
	    public readonly IEnumerable<EntityRelationship> AllRelationships;

		/// <summary>
		/// Initializes a new instance of the RelationshipManager class.
		/// </summary>
		public RelationshipManager(IEntityLight entity, AppDataPackage app, IEnumerable<EntityRelationship> allRelationships)
        {
			_entity = entity;
		    if (app != null)
		        AllRelationships = new CacheChainedIEnumerable<EntityRelationship>(app,
		            () => app.Relationships.ToList());
		    else
		        AllRelationships = allRelationships ?? new List<EntityRelationship>();
		}

		/// <inheritdoc />
		public IEnumerable<IEntity> AllChildren => ChildRelationships().Select(r => r.Child);

	    private IEnumerable<EntityRelationship> ChildRelationships() => AllRelationships.Where(r => r.Parent == _entity);


        /// <inheritdoc />
        public IEnumerable<IEntity> AllParents => ParentRelationships().Select(r => r.Parent);
	    private IEnumerable<EntityRelationship> ParentRelationships() => AllRelationships.Where(r => r.Child == _entity);

        /// <inheritdoc />
        public IChildren Children 
            => _entity is IEntity ? new Children(((IEntity) _entity).Attributes) : null ;



	    #region Relationship-Navigation - experimental?
        [PrivateApi]
	    public List<IEntity> FindChildren(string field = null, string type = null, string useNamedParameters = "xyz", ILog log = null)
	    {
            var wrap = log?.Call("RelMan.FindChildren", $"field:{field}; type:{type}");
	        List<IEntity> rels;
	        if (string.IsNullOrEmpty(field))
	            rels = ChildRelationships().Select(r => r.Child).ToList();
            else
            {
                // If the field doesn't exist, return empty list
                if (!((IEntity) _entity).Attributes.ContainsKey(field))
                    rels = new List<IEntity>();
                else
                    // if it does exist, still catch any situation where it's not a relationship field
                    try
                    {
                        rels = Children[field].ToList();
                    }
                    catch
                    {
                        return new List<IEntity>();
                    }
            }

            // Optionally filter by type
	        if (!string.IsNullOrEmpty(type) && rels.Any())
	            rels = rels.Where(e => e.Type.Is(type)).ToList();
	        wrap?.Invoke(rels.Count.ToString());
	        return rels;
	    }

        [PrivateApi]
	    public List<IEntity> FindParents(string type = null, string field = null, string useNamedParameters = "xyz", ILog log = null)
	    {
	        var wrap = log?.Call("RelMan.FindParents", $"type:{type}; field:{field}");
            var list = ParentRelationships();
	        if (!string.IsNullOrEmpty(type))
	            list = list.Where(r => r.Parent.Type.Is(type));

	        if (string.IsNullOrEmpty(field)) return list.Select(r => r.Parent).ToList();

	        list = list.Where(r => r.Parent.Relationships.FindChildren(field).Any(c => c == r.Child));
	        var result = list.Select(r => r.Parent).ToList();
            wrap?.Invoke(result.Count.ToString());
            return result;
	    }
	    #endregion
    }
}