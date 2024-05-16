using System.Collections;
using System.Collections.Immutable;

namespace ToSic.Eav.Data;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ImmutableSmartList: IImmutableList<IEntity>
{
    #region Constructors / Builders

    /// <summary>
    /// Main way of constructing immutable smart lists - should prevent multiple wrapping
    /// </summary>
    /// <param name="contents"></param>
    /// <returns></returns>
    public static ImmutableSmartList Wrap(IImmutableList<IEntity> contents) 
        => contents as ImmutableSmartList ?? new ImmutableSmartList(contents);

    /// <summary>
    /// Underlying list
    /// </summary>
    protected IImmutableList<IEntity> Contents;

    private ImmutableSmartList(IImmutableList<IEntity> contents) => Contents = contents;

    #endregion

    #region Smart bits / performance

    internal LazyFastAccess Fast => _lfa ??= new(Contents);
    private LazyFastAccess _lfa;

    #endregion

    #region General interface support for IList<T> / IImmutableList<T>

    public IEnumerator<IEntity> GetEnumerator() => Contents.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Contents).GetEnumerator();

    public int Count => Contents.Count;

    public IEntity this[int index] => Contents[index];

    public IImmutableList<IEntity> Clear() => Contents.Clear();

    public int IndexOf(IEntity item, int index, int count, IEqualityComparer<IEntity> equalityComparer) => Contents.IndexOf(item, index, count, equalityComparer);

    public int LastIndexOf(IEntity item, int index, int count, IEqualityComparer<IEntity> equalityComparer) => Contents.LastIndexOf(item, index, count, equalityComparer);

    public IImmutableList<IEntity> Add(IEntity value) => Contents.Add(value);

    public IImmutableList<IEntity> AddRange(IEnumerable<IEntity> items) => Contents.AddRange(items);

    public IImmutableList<IEntity> Insert(int index, IEntity element) => Contents.Insert(index, element);

    public IImmutableList<IEntity> InsertRange(int index, IEnumerable<IEntity> items) => Contents.InsertRange(index, items);

    public IImmutableList<IEntity> Remove(IEntity value, IEqualityComparer<IEntity> equalityComparer) => Contents.Remove(value, equalityComparer);

    public IImmutableList<IEntity> RemoveAll(Predicate<IEntity> match) => Contents.RemoveAll(match);

    public IImmutableList<IEntity> RemoveRange(IEnumerable<IEntity> items, IEqualityComparer<IEntity> equalityComparer) => Contents.RemoveRange(items, equalityComparer);

    public IImmutableList<IEntity> RemoveRange(int index, int count) => Contents.RemoveRange(index, count);

    public IImmutableList<IEntity> RemoveAt(int index) => Contents.RemoveAt(index);

    public IImmutableList<IEntity> SetItem(int index, IEntity value) => Contents.SetItem(index, value);

    public IImmutableList<IEntity> Replace(IEntity oldValue, IEntity newValue, IEqualityComparer<IEntity> equalityComparer) => Contents.Replace(oldValue, newValue, equalityComparer);

    #endregion

}