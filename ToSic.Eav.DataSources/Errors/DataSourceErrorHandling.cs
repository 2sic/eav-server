using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources
{
    internal class DataSourceErrorHandling
    {
        public static string ErrorType = "Error";
        public static string ErrorTitle = "Error";
        public static IEntity CreateErrorEntity(IDataSource source, string stream, string title, string message)
        {
            var values = new Dictionary<string, object>
            {
                {ErrorTitle, "Error: " + title},
                {"SourceName", source?.Name},
                {"SourceLabel", source?.Label },
                {"SourceGuid", source?.Guid },
                {"SourceStream", stream },
                {"Message", message }
            };

            var errorEntity = Build.Entity(values, titleField: ErrorTitle, typeName: ErrorType);
            return errorEntity;
        }

        public static ImmutableArray<IEntity> CreateErrorList(
            string noParameterOrder = Constants.RandomProtectionParameter,
            IDataSource source = null, 
            string title = null, 
            string message = null,
            Exception exception = null,
            string streamName = Constants.DefaultStreamName
            )
        {
            Constants.ProtectAgainstMissingParameterNames(noParameterOrder, "CreateErrorList", "various");

            source?.Log?.Exception(exception);

            // Construct the IEntity and return as ImmutableArray
            var entity = CreateErrorEntity(source, streamName, title, message);
            return new[] { entity }.ToImmutableArray();
        }

    }
}
