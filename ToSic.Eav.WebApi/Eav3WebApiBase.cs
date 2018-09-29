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


        #region App-ID helper - 2018-09-29 2dm disabled as I removed all uses, and this was a dangerous global variable
        //public Eav3WebApiBase(int appId, Log parentLog = null)
        //    : this(parentLog) 
        //    => _appId = appId;

        //[Obsolete("use the new GetAppManager instead, because the fairly trivial set-app-id-then-get feels flaky...")]
        //internal AppManager AppManager => new AppManager(AppId, Log);


        //private const int EmptyAppId = -1;
	    //private int _appId = EmptyAppId;

	    //public int AppId
	    //{
	    //    get
	    //    {
	    //        if(_appId == EmptyAppId)
     //               throw new Exception("AppId not initialized");
	    //        return _appId;
	    //    }
	    //    set => _appId = value;
	    //}



        //public void SetAppId(int? appId)
        //{
        //    if (appId.HasValue)
        //        AppId = appId.Value;
        //}
	    #endregion
        
    }
}