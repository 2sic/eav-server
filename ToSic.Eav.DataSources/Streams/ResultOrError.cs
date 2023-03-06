using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.DataSources
{
    public class ResultOrError<T>
    {
        private readonly Func<IImmutableList<IEntity>> _getError;
        public bool IsOk { get; }
        public bool IsError => !IsOk;
        public T Result { get; }

        public ResultOrError(bool isOk, T result, Func<IImmutableList<IEntity>> getError = null)
        {
            _getError = getError;
            IsOk = isOk;
            Result = result;
        }

        public IImmutableList<IEntity> Errors => _errors.Get(() => _getError?.Invoke() ?? DataSource.EmptyList);
        private readonly GetOnce<IImmutableList<IEntity>> _errors = new GetOnce<IImmutableList<IEntity>>();

        public (IImmutableList<IEntity> Errors, string message) ErrorResult => (Errors, "error");

    }
}
