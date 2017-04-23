using System;
using System.Web.Http;
using Microsoft.Practices.Unity;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Serializers;

namespace ToSic.Eav.WebApi
{
    /// <summary>
    /// Web API Controller for the Pipeline Designer UI
    /// </summary>
    public class Eav3WebApiBase : ApiController
    {

        #region Helpers
        internal IDataSource InitialDS => DataSource.GetInitialDataSource(appId: AppId);

        internal AppManager AppManager => new AppManager(AppId);

        internal IMetaDataSource MetaDS => DataSource.GetMetaDataSource(appId: AppId);

        internal DbDataController _context;
	    internal DbDataController CurrentContext => _context ?? (_context = DbDataController.Instance(appId: AppId));

        // I must keep the serializer so it can be configured from outside if necessary
	    private Serializer _serializer;
	    public Serializer Serializer
	    {
	        get
	        {
	            if (_serializer == null)
	            {
	                _serializer = Factory.Container.Resolve<Serializer>();
	                _serializer.IncludeGuid = true;
	            }
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
	        set { _appId = value; }
	    }

	    #endregion

        #region Constructors

	    public Eav3WebApiBase()
	    {
	        
	    }

	    public Eav3WebApiBase(int appId)
	    {
	        _appId = appId;
	    }
        #endregion

        public void SetAppIdAndUser(int? appId)
        {
            //_appId = appId;
            if (appId.HasValue)
                AppId = appId.Value;

            if (string.IsNullOrWhiteSpace(CurrentContext.UserName))
                SetUser(UserIdentityToken);
        }

        private string _userTokenOverride = null;
        public void SetUser(string userIdentityToken)
        {
            _userTokenOverride = userIdentityToken; // save for later

            // if the context is already initialized, then set it directly (otherwise this will be called later)
            if (_appId != EmptyAppId) // already initialized
                CurrentContext.UserName = userIdentityToken;
        }

        internal string UserIdentityToken => _userTokenOverride ?? System.Web.HttpContext.Current.User.Identity.Name;

    }
}