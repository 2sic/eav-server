#if !NETFRAMEWORK
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using ToSic.Eav.Security.Files;

namespace ToSic.Eav.WebApi.Sys.Helpers.Http;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class HttpUploadedFile(HttpRequest request)
{
    public HttpRequest Request { get; } = request;

    // https://stackoverflow.com/questions/45871479/net-core-2-how-to-check-if-the-request-is-mime-multipart-content
    public bool IsMultipart() => Request.GetMultipartBoundary() != null;

    public bool HasFiles() => Request.Form.Files.Any();

    public int Count => Request.Form.Files.Count;

    public (string, Stream) GetStream(int i = 0)
    {
        var file = Request.Form.Files[i];

        var fileName = FileNames.SanitizeFileName(file?.FileName);

        if (FileNames.IsKnownRiskyExtension(fileName))
            throw new($"File {fileName} has risky file type.");

        return (fileName, file.OpenReadStream());
    }

}
#endif