using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Builder;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    [PrivateApi]
    public class DataSourceErrorHandling
    {
        /// <summary>
        /// Constructor - to find out if it's used anywhere
        /// </summary>
        public DataSourceErrorHandling() {}

        public static string ErrorType = "Error";
        public static string ErrorTitle = "Error";

        public IEntity CreateErrorEntity(IDataSource source, string stream, string title, string message)
        {
            var values = new Dictionary<string, object>
            {
                {ErrorTitle, GenerateTitle(title)},
                {"SourceName", source?.Name},
                {"SourceLabel", source?.Label },
                {"SourceGuid", source?.Guid },
                {"SourceType", source?.GetType().Name },
                {"SourceStream", stream },
                {"Message", message },
                {"DebugNotes", "There should be more details in the insights logs, see https://r.2sxc.org/insights" }
            };

            // #DebugDataSource
            // When debugging I usually want to see where this happens. Feel free to comment in/out as needed
            // System.Diagnostics.Debugger.Break();

            // Don't use the default data builder here, as it needs DI and this object
            // will often be created late when DI is already destroyed
            //var errorEntity = new DataBuilder().Entity(values, titleField: ErrorTitle, typeName: ErrorType);
            var errorEntity = new Entity(DataBuilderInternal.DefaultAppId, DataBuilderInternal.DefaultEntityId,
                new ContentTypeBuilder().Transient(ErrorType), values, ErrorTitle);
            return errorEntity;
        }

        /// <summary>
        /// This must be public so it can be used/verified in testing
        /// </summary>
        public static string GenerateTitle(string title) => "Error: " + title;

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

            source?.Log?.Ex(exception);

            // Construct the IEntity and return as ImmutableArray
            var entity = CreateErrorEntity(source, streamName, title, message);
            return new[] { entity }.ToImmutableArray();
        }

    }
}
