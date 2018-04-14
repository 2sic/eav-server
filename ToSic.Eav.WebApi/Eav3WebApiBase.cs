using System;
using System.Web.Http;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Serializers;

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


        #region Constructors

        public Eav3WebApiBase(Log parentLog, string name = null) => Log.LinkTo(parentLog, name);

        public Eav3WebApiBase(int appId, Log parentLog = null)
            : this(parentLog) 
            => _appId = appId;

        #endregion

        #region Helpers

        internal AppManager AppManager => new AppManager(AppId, Log);


        private DbDataController _dbContext;
	    internal DbDataController CurrentContext => _dbContext ?? (_dbContext = DbDataController.Instance(appId: AppId, parentLog: Log));

        // I must keep the serializer so it can be configured from outside if necessary
	    private Serializer _serializer;
	    public Serializer Serializer
	    {
	        get
	        {
	            if (_serializer != null) return _serializer;
	            _serializer = Factory.Resolve<Serializer>();
	            _serializer.IncludeGuid = true;
	            return _serializer;
	        }
	    }

	    #endregion

        #region App-ID helper

        private const int EmptyAppId = -1;
	    private int _appId = EmptyAppId;

	    public int AppId
	    {
	        get
	        {
	            if(_appId == EmptyAppId)
                    throw new Exception("AppId not initialized");
	            return _appId;
	        }
	        set => _appId = value;
	    }

	    #endregion


        public void SetAppId(int? appId)
        {
            if (appId.HasValue)
                AppId = appId.Value;
        }
        
    }
}