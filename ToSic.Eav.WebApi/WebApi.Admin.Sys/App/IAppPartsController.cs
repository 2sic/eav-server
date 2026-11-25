using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Admin.Sys;

public interface IAppPartsController: IAppAdminWebApiController
{
    /// <summary>
    /// Used to be GET ImportExport/ExportContent
    /// </summary>
    /// <param name="zoneId"></param>
    /// <param name="appId"></param>
    /// <param name="contentTypeIdsString"></param>
    /// <param name="entityIdsString"></param>
    /// <param name="templateIdsString"></param>
    /// <returns></returns>
    // UI test: App administration > Import / Export > Export App Parts > select parts and export.
    THttpResponseType Export(int zoneId, int appId, string contentTypeIdsString, string entityIdsString, string templateIdsString);



    /// <summary>
    /// Used to be GET ImportExport/GetContentInfo
    /// </summary>
    /// <param name="zoneId"></param>
    /// <param name="appId"></param>
    /// <param name="scope"></param>
    /// <returns></returns>
    // UI test: App administration > Import / Export > Export App Parts; opening the dialog loads this overview.
    ExportPartsOverviewDto Get(int zoneId, int appId, string scope);


    /// <summary>
    /// Used to be POST ImportExport/ImportContent
    /// </summary>
    /// <param name="zoneId"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    // UI test: App administration > Import / Export > Import App Parts.
    ImportResult Import(int zoneId, int appId);
}
