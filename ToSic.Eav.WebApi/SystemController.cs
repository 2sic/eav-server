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
	public class SystemController : Eav3WebApiBase
    {
        public SystemController(Log parentLog = null) : base(parentLog, "Api.SysCon")
        {}


        /// <summary>
        /// Get Entities with specified AssignmentObjectTypeId and Key
        /// </summary>
        public IEnumerable<Feature> Features(int appId)
        {
            //AppId = appId;
            return Eav.Configuration.Features.Ui;
        }

    }
}