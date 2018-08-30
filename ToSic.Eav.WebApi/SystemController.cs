using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for MetaData
	/// Metadata-entities (content-items) are additional information about some other object
	/// </summary>
	public abstract class SystemController : Eav3WebApiBase
    {
        protected SystemController(Log parentLog = null) : base(parentLog, "Api.SysCon")
        {}


        /// <summary>
        /// Get Entities with specified AssignmentObjectTypeId and Key
        /// </summary>
        public IEnumerable<Feature> Features(int appId) => GetFeatures(appId);

        public static IEnumerable<Feature> GetFeatures(int appId) => Eav.Configuration.Features.Ui;

    }
}