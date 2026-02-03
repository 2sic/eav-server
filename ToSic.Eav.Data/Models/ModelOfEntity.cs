namespace ToSic.Eav.Models;

// Note: this used to be called EntityBasedType

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public abstract record ModelOfEntity: ModelOfEntityCore, IModelOfEntity
{
    protected ModelOfEntity() { }
    protected ModelOfEntity(IEntity entity) : base(entity) { }

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public virtual string Title => field
        ??= Entity?.GetBestTitle() ?? "";

    /// <inheritdoc />
    public int Id => Entity?.EntityId ?? 0;

    /// <inheritdoc />
    public Guid Guid => Entity?.EntityGuid ?? Guid.Empty;
}
