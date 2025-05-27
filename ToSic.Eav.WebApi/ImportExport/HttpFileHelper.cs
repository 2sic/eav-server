using System.Net;
using System.Net.Http;
using System.Text;

namespace ToSic.Eav.WebApi.ImportExport;

internal static class HttpFileHelper
{
    public static HttpResponseMessage GetAttachmentHttpResponseMessage(string fileName, string fileType, Stream fileContent)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {Content = new StreamContent(fileContent)};
        response.Content.Headers.ContentType = new(fileType);
        response.Content.Headers.ContentLength = fileContent.Length;
        response.Content.Headers.ContentDisposition = new("attachment")
        {
            FileName = fileName
        };
        return response;
    }

    public static HttpResponseMessage GetAttachmentHttpResponseMessage(string fileName, string fileType, string fileContent)
    {
        var fileBytes = Encoding.UTF8.GetBytes(fileContent);
        return GetAttachmentHttpResponseMessage(fileName, fileType, new MemoryStream(fileBytes));
    }

}