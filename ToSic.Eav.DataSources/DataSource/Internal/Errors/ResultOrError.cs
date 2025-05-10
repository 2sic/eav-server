namespace ToSic.Eav.DataSource.Internal.Errors;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ResultOrError<T>(bool isOk, T result, IImmutableList<IEntity> errors = null)
{
    public bool IsOk { get; } = isOk;
    public bool IsError => !IsOk;
    public T Result { get; } = result;

    public IImmutableList<IEntity> Errors { get; } = errors?? [];
}