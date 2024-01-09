using System.Collections.Immutable;
using ToSic.Eav.Data;
using static ToSic.Eav.DataSource.Internal.DataSourceConstants;

namespace ToSic.Eav.DataSource.Internal.Errors;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ResultOrError<T>(bool isOk, T result, IImmutableList<IEntity> errors = null)
{
    public bool IsOk { get; } = isOk;
    public bool IsError => !IsOk;
    public T Result { get; } = result;

    public IImmutableList<IEntity> Errors { get; } = errors?? EmptyList;
}