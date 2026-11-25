using ToSic.Eav.ImportExport.Sys;

namespace ToSic.Eav.WebApi.Admin.Sys;

public interface ITypeController: IAppAdminWebApiController
{
    /// <summary>
    /// Create a ghost-content-type (very advanced feature)
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="sourceNameId"></param>
    /// <returns></returns>
    // UI test: App administration > Data > Add Content Type > create a ghost / inherited type.
    bool AddGhost(int appId, string sourceNameId);

    /// <summary>
    /// Delete a content-type from an app
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="staticName"></param>
    /// <returns></returns>
    // UI test: App administration > Data > delete a content type.
    bool Delete(int appId, string staticName);

    // 2019-11-15 2dm special change: item to be Dictionary<string, object> because in DNN 9.4
    // it causes problems when a content-type has metadata, where a value then is a deeper object
    // in future, the JS front-end should send something clearer and not the whole object
    // UI test: App administration > Data > create or edit a content type and save.
    bool Save(int appId, Dictionary<string, object?> item);

    /// <summary>
    /// Mark a field as being the title field
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="contentTypeId"></param>
    /// <param name="attributeId"></param>
    // UI test: App administration > Data > content type fields > Set as title field.
    void SetTitle(int appId, int contentTypeId, int attributeId);

    // UI test: App administration > Data > content-type menu > Export JSON.
    THttpResponseType Json(int appId, string name);

    /// <summary>
    /// Used to be POST ImportExport/ImportContent
    /// </summary>
    /// <remarks>
    /// New in 2sxc 11.07
    /// </remarks>
    /// <returns></returns>
    // UI test: App administration > Data > Import Content Type.
    ImportResult Import(int zoneId, int appId);

    /// <summary>
    /// Json Bundle Export
    /// </summary>
    /// <remarks>
    /// New in 2sxc v15
    /// </remarks>
    /// <returns></returns>
    // UI test: Not called by eav-ui; data-bundle export uses admin/data/BundleExport instead.
    THttpResponseType JsonBundleExport(int appId, Guid exportConfiguration, int indentation = 0);
}
