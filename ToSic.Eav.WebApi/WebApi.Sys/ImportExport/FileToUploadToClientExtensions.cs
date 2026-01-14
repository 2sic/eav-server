#if NETFRAMEWORK
using System.Net;
using System.Net.Http;
#endif

namespace ToSic.Eav.WebApi.Sys.ImportExport;

public static class FileToUploadToClientExtensions
{
#if NETFRAMEWORK

    public static THttpResponseType ToHttpResponse(this FileToUploadToClient file)
    {
        var fileContent = new MemoryStream(file.FileBytes);
        var response = new HttpResponseMessage(HttpStatusCode.OK) {Content = new StreamContent(fileContent)};
        response.Content.Headers.ContentType = new(file.ContentType);
        response.Content.Headers.ContentLength = fileContent.Length;
        response.Content.Headers.ContentDisposition = new("attachment")
        {
            FileName = file.FileName
        };
        return response;
    }
#else
    public static THttpResponseType ToHttpResponse(this FileToUploadToClient file) =>
        new Microsoft.AspNetCore.Mvc.FileContentResult(file.FileBytes, MimeTypeConstants.FallbackType)
        {
            FileDownloadName = file.FileName
        };
#endif

    //public static FileToUploadToClient ToHttpResponse(this FileToUploadToClient file, string fileName, string fileType, string fileContent)
    //{
    //    var fileBytes = Encoding.UTF8.GetBytes(fileContent);
    //    return ToHttpResponse(new () { FileName = fileName, FileType = fileType, FileBytes = fileBytes });
    //}

}