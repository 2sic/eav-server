using System.Web.Http;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.WebApi
{
    /// <inheritdoc />
    /// <summary>
    /// Web API Controller for the Pipeline Designer UI
    /// </summary>
    public class Eav3WebApiBase : ApiController
    {
        #region Logging
        protected Log Log = new Log("Eav.ApiCon");
        #endregion

        public Eav3WebApiBase(Log parentLog, string name = null) => Log.LinkTo(parentLog, name);
        
    }
}