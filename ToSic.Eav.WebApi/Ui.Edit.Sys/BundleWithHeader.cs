namespace ToSic.Eav.Ui.Edit.Sys;

public record BundleWithHeader
{
    public required ItemIdentifier Header { get; init; }     
}

public record BundleWithHeaderOptional<TEntity>
{
    public ItemIdentifier? Header { get; init; }
    public TEntity? Entity { get; init; }

}
public record BundleWithHeader<TEntity> : BundleWithHeader
{
    public required TEntity Entity { get; init; }

}
