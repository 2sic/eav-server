namespace ToSic.Eav.Models;

// Note: this used to be called EntityBasedType

/// <summary>
/// A basic model (record) of entities. It extends the core implementation by providing Id, Guid and Title by default.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public abstract record ModelOfEntityBasic: ModelOfEntity, IModelOfEntityBasic
{
    /// <summary>
    /// Empty constructor, so it can be inherited without having to specify a constructor.
    /// </summary>
    protected ModelOfEntityBasic() { }

    /// <summary>
    /// Constructor which already includes the data to wrap; rarely used.
    /// </summary>
    /// <param name="entity"></param>
    protected ModelOfEntityBasic(IEntity entity) : base(entity) { }

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public virtual string Title => field
        ??= Entity?.GetBestTitle() ?? "";

    /// <inheritdoc />
    public int Id => Entity?.EntityId ?? 0;

    /// <inheritdoc />
    public Guid Guid => Entity?.EntityGuid ?? Guid.Empty;
}
