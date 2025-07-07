namespace ToSic.Eav.DataSource.Sys.Errors;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record ResultOrError<T>(bool IsOk, T? Result, IImmutableList<IEntity>? Errors = null)
{
    public IImmutableList<IEntity> ErrorsSafe() => Errors ?? [];

    // ReSharper disable once RedundantExplicitPositionalPropertyDeclaration - needed since it uses nameOf, otherwise it can't compile
    [MemberNotNullWhen(true, nameof(Result))]
    public bool IsOk { get; init; } = IsOk;
}