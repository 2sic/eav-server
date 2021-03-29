using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using static ToSic.Eav.DataSources.DataSourceErrorHandling;

namespace ToSic.Eav.DataSources
{
    public partial class DataSourceBase
    {
        protected ImmutableArray<IEntity> ExceptionStream;

        protected ImmutableArray<IEntity> SetException(string title, string message, Exception ex = null)
        {
            var result = CreateErrorList(source: this, title: title, message: message, exception: ex);
            ExceptionStream = result;
            return result;
        }
    }
}
