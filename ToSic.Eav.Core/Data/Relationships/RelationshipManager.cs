using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Logging;
using ToSic.Lib.Documentation;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Used to get relationships between entities.
	/// </summary>
	public class RelationshipManager: IRelationshipManager
	{
        // special note: ATM everything is an IEntity, so EntityLight is currently not supported

        private readonly IEntityLight _entity;
	    public IEnumerable<EntityRelationship> AllRelationships { get; }

        private AppState App;

		/// <summary>
		/// Initializes a new instance of the RelationshipManager class.
		/// </summary>
		public RelationshipManager(IEntityLight entity, AppState app, IEnumerable<EntityRelationship> allRelationships)
        {
			_entity = entity;
            App = app;
		    if (app != null)
		        AllRelationships = new SynchronizedList<EntityRelationship>(app,
		            () => app.Relationships.List);
            else
		        AllRelationships = allRelationships ?? new List<EntityRelationship>();
		}

		/// <inheritdoc />
		public IEnumerable<IEntity> AllChildren => ChildRelationships().Select(r => r.Child);

	    private IImmutableList<EntityRelationship> ChildRelationships()
        {
            if (_childRelationships != null) return _childRelationships.List;
            Func<IImmutableList<EntityRelationship>> getChildren = () => AllRelationships.Where(r => r.Parent == _entity).ToImmutableArray();
            if (App == null) return getChildren.Invoke();
            _childRelationships = new SynchronizedList<EntityRelationship>(App, getChildren);
            return _childRelationships.List;
        }

        private SynchronizedList<EntityRelationship> _childRelationships;


        /// <inheritdoc />
        // note: don't cache the result, as it's already cache-chained
        public IEnumerable<IEntity> AllParents 
            => ParentRelationships().Select(r => r.Parent);
        // note: don't cache the result, as it's already cache-chained
	    private IImmutableList<EntityRelationship> ParentRelationships()
        {
            if (_parentRelationships != null) return _parentRelationships.List;
            Func<IImmutableList<EntityRelationship>> getParents = () => AllRelationships.Where(r => r.Child == _entity).ToImmutableArray();// .ToList();
            if (App == null) return getParents.Invoke(); // return AllRelationships.Where(r => r.Child == _entity);
            _parentRelationships = new SynchronizedList<EntityRelationship>(App, getParents);
            return _parentRelationships.List;
        }
        private SynchronizedList<EntityRelationship> _parentRelationships;

        /// <inheritdoc />
        [PrivateApi]
        public IRelationshipChildren Children 
            => _entity is IEntity entity ? new RelationshipChildren(entity.Attributes) : null ;



	    #region Relationship-Navigation
        /// <inheritdoc />
	    public List<IEntity> FindChildren(string field = null, string type = null, ILog log = null)
	    {
            var wrap = log?.Fn($"field:{field}; type:{type}");
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
	            rels = rels.OfType(type).ToList();
	        wrap?.Done(rels.Count.ToString());
	        return rels;
	    }

        /// <inheritdoc />
        public List<IEntity> FindParents(string type = null, string field = null, ILog log = null)
	    {
	        var wrap = log?.Fn($"type:{type}; field:{field}");
            var list = ParentRelationships() as IEnumerable<EntityRelationship>;
	        if (!string.IsNullOrEmpty(type))
	            list = list.Where(r => r.Parent.Type.Is(type));

	        if (string.IsNullOrEmpty(field))
            {
                var rez = list.Select(r => r.Parent).ToList();
                wrap?.Done(rez.Count.ToString());
                return rez;
            };

	        list = list.Where(r => r.Parent.Relationships.FindChildren(field).Any(c => c == r.Child));
	        var result = list.Select(r => r.Parent).ToList();
            wrap?.Done(result.Count.ToString());
            return result;
	    }
		#endregion

		//#region Relationship Information
  //      [PrivateApi]
  //      public IEnumerable<string> Fields()
  //      {
  //          return null;
  //      }

  //      #endregion
    }
}