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
    /// <summary>
    /// An Errors-helper which is automatically available on all <see cref="DataSource"/> objects.
    ///
    /// It helps create a stream of standardized error entities.
    /// </summary>
    [PublicApi]
    public class DataSourceErrorHelper
    {

        /// <summary>
        /// Constructor - to find out if it's used anywhere
        /// </summary>
        public DataSourceErrorHelper(DataBuilder builder)
        {
            _builder = builder;
        }
        private readonly DataBuilder _builder;

        public static string ErrorContentType = "Error";
        public static string ErrorTitleField = "Error";


        /// <summary>
        /// Create a stream containing an error entity.
        /// </summary>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="title">Error title</param>
        /// <param name="message">Error message</param>
        /// <param name="exception">Exception (if there was an exception)</param>
        /// <param name="source">The DataSource which created this error. If provided, will allow the message to contain more details.</param>
        /// <param name="streamName">The stream name. If provided, will allow the message to contain more details.</param>
        /// <returns></returns>
        public IImmutableList<IEntity> Create(
            string noParamOrder = Parameters.Protector,
            string title = default, 
            string message = default,
            Exception exception = default,
            IDataSource source = default, 
            string streamName = Constants.DefaultStreamName
            )
        {
            Parameters.ProtectAgainstMissingParameterNames(noParamOrder, nameof(Create), "various");

            source?.Log?.Ex(exception);

            // Construct the IEntity and return as Immutable
            var entity = CreateErrorEntity(source, streamName, title, message);
            return new[] { entity }.ToImmutableList();
        }

        [PrivateApi("usually not needed externally")]
        public IEntity CreateErrorEntity(IDataSource source, string stream, string title, string message)
        {
            var values = new Dictionary<string, object>
            {
                {ErrorTitleField, GenerateTitle(title)},
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
            var errorEntity = _builder.Entity.Create(appId: DataFactory.DefaultAppId, entityId: DataFactory.DefaultEntityId,
                contentType: _builder.ContentType.Transient(ErrorContentType),
                attributes: _builder.Attribute.Create(values),
                titleField: ErrorTitleField);
            return errorEntity;
        }

        /// <summary>
        /// This must be internal so it can be used/verified in testing
        /// </summary>
        [PrivateApi("only internal for testing")]
        internal static string GenerateTitle(string title) => "Error: " + title;

    }
}
