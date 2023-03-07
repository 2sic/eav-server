using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.DataSources
{
    public class ListOrError
    {
        public ListOrError(bool isError, IImmutableList<IEntity> getList = null, IImmutableList<IEntity> getError = null)
        {
            _getList = getList;
            _getError = getError;
            IsError = isError;
        }
        private readonly IImmutableList<IEntity> _getList;
        private readonly IImmutableList<IEntity> _getError;

        public bool IsError { get; }

        public IImmutableList<IEntity> List => _list.Get(() => _getList ?? DataSource.EmptyList);
        private readonly GetOnce<IImmutableList<IEntity>> _list = new GetOnce<IImmutableList<IEntity>>();

        public IImmutableList<IEntity> Errors => _errors.Get(() => _getError ?? DataSource.EmptyList);
        private readonly GetOnce<IImmutableList<IEntity>> _errors = new GetOnce<IImmutableList<IEntity>>();

        public (IImmutableList<IEntity> Errors, string message) ErrorResult => (Errors, "error");
    }
}
