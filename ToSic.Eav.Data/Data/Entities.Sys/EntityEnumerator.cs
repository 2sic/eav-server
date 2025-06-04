using System.Collections;

namespace ToSic.Eav.Data.Entities.Sys;

/// <inheritdoc />
/// <remarks>Source: http://msdn.microsoft.com/en-us/library/system.collections.ienumerable.getenumerator.aspx </remarks>
[PrivateApi]
internal class EntityEnumerator(List<IEntity> entities) : IEnumerator<IEntity>
{
    private int _position = -1;

    public void Dispose()
    {
    }

    public bool MoveNext()
    {
        _position++;
        return _position < entities.Count;
    }

    public void Reset() => _position = -1;

    public IEntity Current
    {
        get
        {
            try
            {
                return entities[_position];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }

    object IEnumerator.Current => Current;
}