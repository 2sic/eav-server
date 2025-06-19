namespace ToSic.Eav.DataSource.Internal.Errors;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record ResultOrError<T>(bool IsOk, T? Result, IImmutableList<IEntity>? Errors = null)
{
    public bool IsError() => !IsOk;

    public IImmutableList<IEntity> ErrorsSafe() => Errors ?? [];
}