#if NETFRAMEWORK

using System.Net.Http;
using System.Web;
using Microsoft.EntityFrameworkCore.Internal;
using ToSic.Eav.Security.Files;

namespace ToSic.Eav.WebApi.Sys.Helpers.Http;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class HttpUploadedFile(HttpRequestMessage requestMessage, HttpRequest request)
{
    public HttpRequestMessage RequestMessage { get; } = requestMessage;
    public HttpRequest Request { get; } = request;

    public bool IsMultipart() => RequestMessage.Content.IsMimeMultipartContent();

    public bool HasFiles() => Request.Files.Any();

    public int Count => Request.Files.Count;

    public (string, Stream) GetStream(int i = 0)
    {
        var file = Request.Files[i];

        var fileName = FileNames.SanitizeFileName(file?.FileName);

        if (FileNames.IsKnownRiskyExtension(fileName))
            throw new($"File {fileName} has risky file type.");

        return (fileName, file?.InputStream);
    }
}

#endif