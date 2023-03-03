using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Data.Factory;
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
        public DataSourceErrorHandling(MultiBuilder builder)
        {
            _builder = builder;
        }
        private readonly MultiBuilder _builder;

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
            //var errorEntity = new DataFactory().Entity(values, titleField: ErrorTitle, typeName: ErrorType);
            var errorEntity = _builder.Entity.Create(appId: DataFactory.DefaultAppId, entityId: DataFactory.DefaultEntityId,
                contentType: _builder.ContentType.Transient(ErrorType),
                attributes: _builder.Attribute.Create(values),
                titleField: ErrorTitle);
            return errorEntity;
        }

        /// <summary>
        /// This must be internal so it can be used/verified in testing
        /// </summary>
        internal static string GenerateTitle(string title) => "Error: " + title;

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
