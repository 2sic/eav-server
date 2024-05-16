namespace ToSic.Eav.WebApi;

/// <inheritdoc />
/// <summary>
/// Web API Controller for ContentTypes
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ContentTypeDtoService(
    GenWorkPlus<WorkEntities> workEntities,
    GenWorkBasic<WorkAttributes> attributes,
    Generator<ConvertAttributeToDto> convAttrDto,
    ConvertContentTypeToDto convTypeDto)
    : ServiceBase("Api.EavCTC", connect: [attributes, convAttrDto, convTypeDto, workEntities])
{

    #region Content-Type Get, Delete, Save

    public IList<ContentTypeDto> List(int appId, string scope = null, bool withStatistics = false)
    {
        var l = Log.Fn<IList<ContentTypeDto>>($"scope:{scope}, stats:{withStatistics}");
        var appCtxPlus = workEntities.CtxSvc.ContextPlus(appId);

        // 2023-11-08 Will disable this now, as I believe there is no case
        // ...where the data can be loaded into memory and NOT have initialized already. 
        // https://github.com/2sic/2sxc/issues/3203
        // If no problems arise till 2024-Q1, remove this entire block 
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
        var appEntities = workEntities.New(appCtxPlus);

        // get all types
        var allTypes = appCtxPlus.AppState.ContentTypes
            .OfScope(scope, true)
            .ToList();

        var ofScopeAndOrdered = allTypes
            .Where(t => t.Scope == scope)
            .OrderBy(t => t.Name)
            .ToList();

        var filteredType = ofScopeAndOrdered
            .Select(t => convTypeDto.Convert(t, appEntities.Get(t.Name).Count()))
            .ToList(); // must convert to list, otherwise it happens late when DI isn't available any more

        return l.ReturnAsOk(filteredType);
    }
        

    public ContentTypeDto GetSingle(int appId, string contentTypeStaticName, string scope = null)
    {
        var l = Log.Fn<ContentTypeDto>($"a#{appId}, type:{contentTypeStaticName}, scope:{scope}");
        var appCtxPlus = workEntities.CtxSvc.ContextPlus(appId);
        var ct = appCtxPlus.AppState.GetContentType(contentTypeStaticName);
        return l.Return(convTypeDto.Convert(ct));
    }

    #endregion

    #region Fields - Get, Reorder, Data-Types (for dropdown), etc.

    /// <summary>
    /// Returns the configuration for a content type
    /// </summary>
    public IEnumerable<ContentTypeFieldDto> GetFields(int appId, string staticName)
    {
        var fields = attributes.New(appId).GetFields(staticName);
        return convAttrDto.New().Init(appId, false).Convert(fields);
    }


    public IEnumerable<ContentTypeFieldDto> GetSharedFields(int appId, int attributeId)
    {
        var fields = attributes.New(appId).GetSharedFields(attributeId);
        return convAttrDto.New().Init(appId, true).Convert(fields);
    }

    #endregion

}