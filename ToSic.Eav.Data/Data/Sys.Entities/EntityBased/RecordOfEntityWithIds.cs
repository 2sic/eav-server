namespace ToSic.Eav.Data.Sys.Entities;

public abstract record RecordOfEntityWithIds: RecordOfEntityBase, IModelOfEntity
{
    protected RecordOfEntityWithIds() { }
    protected RecordOfEntityWithIds(IEntity entity) : base(entity) { }

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public virtual string Title => field
        ??= Entity?.GetBestTitle() ?? "";

    /// <inheritdoc />
    public int Id => Entity?.EntityId ?? 0;

    /// <inheritdoc />
    public Guid Guid => Entity?.EntityGuid ?? Guid.Empty;
}
