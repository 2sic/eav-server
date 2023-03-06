using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.DataSources
{
    public class GetStream
    {
        public GetStream(bool isError, Func<IImmutableList<IEntity>> getList = null, Func<IImmutableList<IEntity>> getError = null)
        {
            _getList = getList;
            _getError = getError;
            IsError = isError;
        }
        private readonly Func<IImmutableList<IEntity>> _getList;
        private readonly Func<IImmutableList<IEntity>> _getError;

        public bool IsError { get; }

        public IImmutableList<IEntity> List => _list.Get(() => _getList?.Invoke() ?? DataSource.EmptyList);
        private readonly GetOnce<IImmutableList<IEntity>> _list = new GetOnce<IImmutableList<IEntity>>();

        public IImmutableList<IEntity> Errors => _errors.Get(() => _getError?.Invoke() ?? DataSource.EmptyList);
        private readonly GetOnce<IImmutableList<IEntity>> _errors = new GetOnce<IImmutableList<IEntity>>();

        public (IImmutableList<IEntity> Errors, string message) ErrorResult => (Errors, "error");
    }
}
