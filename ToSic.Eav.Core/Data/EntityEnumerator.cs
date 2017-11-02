using System;
using System.Collections;
using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <inheritdoc />
    /// <remarks>Source: http://msdn.microsoft.com/en-us/library/system.collections.ienumerable.getenumerator.aspx </remarks>
    internal class EntityEnumerator : IEnumerator<IEntity>
    {
        private readonly List<IEntity> _entities;
        private int _position = -1;

        public EntityEnumerator(List<IEntity> entities)
        {
            _entities = entities;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            _position++;
            return _position < _entities.Count;
        }

        public void Reset() => _position = -1;

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

        object IEnumerator.Current => Current;
    }
}