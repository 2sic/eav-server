using System.Collections.Immutable;
using ToSic.Eav.Data;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSource
{
    public class ResultOrError<T>
    {
        public bool IsOk { get; }
        public bool IsError => !IsOk;
        public T Result { get; }

        public ResultOrError(bool isOk, T result, IImmutableList<IEntity> errors = null)
        {
            Errors = errors?? EmptyList;
            IsOk = isOk;
            Result = result;
        }

        public IImmutableList<IEntity> Errors { get; }
    }
}
