using ToSic.Eav.WebApi.Dto;
#if NETFRAMEWORK
using THttpResponseType = System.Net.Http.HttpResponseMessage;
#else
using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
#endif

namespace ToSic.Eav.WebApi.Admin
{
    public interface IAppPartsController
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
        THttpResponseType Export(int zoneId, int appId, string contentTypeIdsString, string entityIdsString, string templateIdsString);



        /// <summary>
        /// Used to be GET ImportExport/GetContentInfo
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        ExportPartsOverviewDto Get(int zoneId, int appId, string scope);


        /// <summary>
        /// Used to be POST ImportExport/ImportContent
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        ImportResultDto Import(int zoneId, int appId);
    }
}