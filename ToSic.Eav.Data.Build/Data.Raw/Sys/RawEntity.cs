using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// A ready-to-use <see cref="IRawEntity"/> which receives all the data in the constructor.
///
/// Use this for scenarios where you don't want to create your own IRawEntity but wish to return this kind of typed object.
/// </summary>
/// <remarks>
/// * Added in 15.04 - was public for a while, but never quite stable, believe never used
/// * Made private again in 16.09
/// * Moved to .Sys ca. v20 - no complaints, so seems to be unused
/// * Changed to be record instead of class v22 (WIP)
/// </remarks>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record RawEntity: IRawEntity, IRelationshipKeys, IHasMetadata
{
    #region Core Properties Id, Guid, Created, Modified

    public virtual int Id
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    }

    public virtual Guid Guid
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    } = Guid.Empty;

    public virtual DateTime Created
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    } = DateTime.Now;

    public virtual DateTime Modified
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    } = DateTime.Now;

    #endregion


    #region Values

    public IDictionary<string, object?> Values
    {
        get => field ??= GetValues();
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set => field = value?.ToInvariant();
#else
        init => field = value?.ToInvariant();
#endif
    }

    /// <summary>
    /// Override this method to provide the values for the entity. By default, it returns an empty dictionary.
    /// </summary>
    /// <remarks>
    /// This is needed because we don't want to make <see cref="Values"/> virtual, since any implementation
    /// would also have to implement the init, defeating the purpose.
    /// </remarks>
    /// <returns></returns>
    protected virtual IDictionary<string, object?> GetValues() =>
        new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);

    #endregion


    #region Optional Relationships and Metadata

    public IEnumerable<object>? RelationshipKeys { get; init; }

    /// <summary>
    /// WIP experimental v18.02 - trying to get content-type metadata into the raw entity
    /// </summary>
    /// <remarks>
    /// Explicitly nullable, even if the IHasMetadata interface would not expect this.
    /// </remarks>
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    public IMetadata? Metadata { get; init; }
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

    #endregion

}