using System;
using System.Web.Http;
using Microsoft.Practices.Unity;
using ToSic.Eav.BLL;
using ToSic.Eav.DataSources;
using ToSic.Eav.Serializers;

namespace ToSic.Eav.WebApi
{
    /// <summary>
    /// Web API Controller for the Pipeline Designer UI
    /// </summary>
    public class Eav3WebApiBase : ApiController
    {

        #region Helpers
        internal IDataSource InitialDS 
	    {
            get { return DataSource.GetInitialDataSource(appId: AppId); }
	    }

	    internal IMetaDataSource MetaDS
	    {
	        get { return DataSource.GetMetaDataSource(appId: AppId); }
	    }

	    internal EavDataController _context;
	    internal EavDataController CurrentContext
	    {
	        get
	        {
	            if (_context == null)
	                _context = EavDataController.Instance(appId: AppId);
	            return _context;
	        }
	    }

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

	    private int _appId = -1;

	    public int AppId
	    {
	        get
	        {
	            if(_appId == -1)
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

        public void SetAppIdAndUser(int appId)
        {
            _appId = appId;
            CurrentContext.UserName = System.Web.HttpContext.Current.User.Identity.Name;
        }


    }
}