using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    [PrivateApi]
    public class DataSourceErrorHandling
    {
        public DataSourceErrorHandling()
        {
        }
        
        public static string ErrorType = "Error";
        public static string ErrorTitle = "Error";
        public IEntity CreateErrorEntity(IDataSource source, string stream, string title, string message)
        {
            var values = new Dictionary<string, object>
            {
                {ErrorTitle, "Error: " + title},
                {"SourceName", source?.Name},
                {"SourceLabel", source?.Label },
                {"SourceGuid", source?.Guid },
                {"SourceType", source?.GetType().Name },
                {"SourceStream", stream },
                {"Message", message },
                {"DebugNotes", "There should be more details in the insights logs, see https://r.2sxc.org/insights" }
            };

            //var errorEntity = _dataBuilderLazy.Value.Entity(values, titleField: ErrorTitle, typeName: ErrorType);
            var errorEntity = new DataBuilder().Entity(values, titleField: ErrorTitle, typeName: ErrorType);
            return errorEntity;
        }

        public ImmutableArray<IEntity> CreateErrorList(
            string noParamOrder = Parameters.Protector,
            IDataSource source = null, 
            string title = null, 
            string message = null,
            Exception exception = null,
            string streamName = Constants.DefaultStreamName
            )
        {
            Parameters.ProtectAgainstMissingParameterNames(noParamOrder, "CreateErrorList", "various");

            source?.Log?.Exception(exception);

            // Construct the IEntity and return as ImmutableArray
            var entity = CreateErrorEntity(source, streamName, title, message);
            return new[] { entity }.ToImmutableArray();
        }

    }
}
