using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Dto;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using ToSic.Eav.Apps.Work;

namespace ToSic.Eav.WebApi
{
    /// <inheritdoc />
    /// <summary>
    /// Web API Controller for ContentTypes
    /// </summary>
    public partial class ContentTypeApi : ServiceBase
    {

        #region Constructor / DI

        public ContentTypeApi(
            AppInitializedChecker appInitializedChecker,
            GenWorkPlus<WorkEntities> workEntities,
            GenWorkBasic<WorkAttributes> attributes,
            Generator<ConvertAttributeToDto> convAttrDto,
            ConvertContentTypeToDto convTypeDto) : base("Api.EavCTC")
        {
            ConnectServices(
                _attributes = attributes,
                _appInitializedChecker = appInitializedChecker,
                _convAttrDto = convAttrDto,
                _convTypeDto = convTypeDto,
                _workEntities = workEntities
            );
        }

        private readonly ConvertContentTypeToDto _convTypeDto;
        private readonly Generator<ConvertAttributeToDto> _convAttrDto;
        private readonly GenWorkBasic<WorkAttributes> _attributes;
        private readonly GenWorkPlus<WorkEntities> _workEntities;
        private readonly AppInitializedChecker _appInitializedChecker;

        public ContentTypeApi Init(int appId)
        {
            var l = Log.Fn<ContentTypeApi>($"{appId}");
            _appId = appId;
            _appCtxPlus = _workEntities.CtxSvc.ContextPlus(appId);
            return l.Return(this);
        }

        private int _appId;
        private IAppWorkCtxPlus _appCtxPlus;

        #endregion

        #region Content-Type Get, Delete, Save

        public IEnumerable<ContentTypeDto> List(string scope = null, bool withStatistics = false)
        {
            var l = Log.Fn<IEnumerable<ContentTypeDto>>($"scope:{scope}, stats:{withStatistics}");

            // 2023-11-08 Will disable this now, as I believe there is no case
            // ...where the data can be loaded into memory and NOT have initialized already. 
            // https://github.com/2sic/2sxc/issues/3203

            //// 2020-01-15 2sxc 10.27.00 Special side-effect, pre-generate the resources, settings etc. if they didn't exist yet
            //// this is important on "Content" apps, because these don't auto-initialize when loading from the DB
            //// so for these, we must pre-ensure that the app is initialized as needed, if they 
            //// are editing the resources etc. 
            //if (scope == Data.Scopes.App)
            //{
            //    l.A($"is scope {scope}, will do extra processing");
            //    // make sure additional settings etc. exist
            //    _appInitializedChecker.EnsureAppConfiguredAndInformIfRefreshNeeded(_appCtxPlus.AppState, null, new CodeRef(), Log); 
            //}

            // should use app-manager and return each type 1x only
            var appEntities = _workEntities.New(_appCtxPlus);

            // get all types
            var allTypes = _appCtxPlus.AppState.ContentTypes.OfScope(scope, true);

            var filteredType = allTypes.Where(t => t.Scope == scope)
                .OrderBy(t => t.Name)
                .Select(t => _convTypeDto.Convert(t, appEntities.Get(t.Name).Count()));
            return l.ReturnAsOk(filteredType);
	    }
        

	    public ContentTypeDto GetSingle(string contentTypeStaticName, string scope = null)
	    {
	        var l = Log.Fn<ContentTypeDto>($"a#{_appId}, type:{contentTypeStaticName}, scope:{scope}");
            var ct = _appCtxPlus.AppState.GetContentType(contentTypeStaticName);
            return l.Return(_convTypeDto.Convert(ct));
        }

        #endregion

        #region Fields - Get, Reorder, Data-Types (for dropdown), etc.

        /// <summary>
        /// Returns the configuration for a content type
        /// </summary>
        public IEnumerable<ContentTypeFieldDto> GetFields(string staticName)
        {
            var l = Log.Fn<IEnumerable<ContentTypeFieldDto>>($"get fields a#{_appId}, type:{staticName}");
            
            var fields = _attributes.New(_appCtxPlus).GetFields(staticName);

            return l.Return(_convAttrDto.New().Init(_appId, false).Convert(fields));
        }


        public IEnumerable<ContentTypeFieldDto> GetSharedFields()
        {
            var l = Log.Fn<IEnumerable<ContentTypeFieldDto>>($"get shared fields a#{_appId}");

            var fields = _attributes.New(_appId).GetSharedFields();

            return l.Return(_convAttrDto.New().Init(_appId, true).Convert(fields));
        }
        
        #endregion

    }
}