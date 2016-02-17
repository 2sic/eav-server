using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents Relationships to Child Entities
    /// </summary>
    public class EntityRelationship : IEnumerable<IEntity>
    {
        /// <summary>
        /// Blank value, just for marking the list as empty
        /// </summary>
        private static readonly int?[] EntityIdsEmpty = new int?[0];

        /// <summary>
        /// List of Child EntityIds
        /// </summary>
        public IEnumerable<int?> EntityIds { get; private set; }

        private readonly IDeferredEntitiesList _fullEntityList;
        private List<IEntity> _entities;

        /// <summary>
        /// Initializes a new instance of the EntityRelationship class.
        /// </summary>
        /// <param name="fullEntitiesListForLookup">DataSource to retrieve child entities</param>
        /// <param name="entityIds">List of IDs to initialize with</param>
        public EntityRelationship(IDeferredEntitiesList fullEntitiesListForLookup, IEnumerable<int?> entityIds = null)
        {
            EntityIds = entityIds ?? EntityIdsEmpty;
            _fullEntityList = fullEntitiesListForLookup; 
        }

        public override string ToString()
        {
            return EntityIds == null ? string.Empty : string.Join(", ", EntityIds.Select(e => e));
        }

        public IEnumerator<IEntity> GetEnumerator()
        {
            // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
            if (_entities == null)
                //_entities = _source == null ? new List<IEntity>() : _source.Out[Constants.DefaultStreamName].List.Where(l => EntityIds.Contains(l.Key)).Select(l => l.Value).ToList();
                _entities = _fullEntityList == null ? new List<IEntity>() 
                    : EntityIds.Select(l => l.HasValue 
                    ? (_fullEntityList.List.ContainsKey(l.Value) ? _fullEntityList.List[l.Value] : null) // special: in rare cases, the entity has been deleted and is therefor missing
                    : null).ToList();

            return new EntityEnum(_entities);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <remarks>Source: http://msdn.microsoft.com/en-us/library/system.collections.ienumerable.getenumerator.aspx </remarks>
        class EntityEnum : IEnumerator<IEntity>
        {
            private readonly List<IEntity> _entities;
            private int _position = -1;

            public EntityEnum(List<IEntity> entities)
            {
                _entities = entities;
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                _position++;
                return (_position < _entities.Count);
            }

            public void Reset()
            {
                _position = -1;
            }

            public IEntity Current
            {
                get
                {
                    try
                    {
                        return _entities[_position];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}