using System.Net.Http;
using ToSic.Eav.Identity;
using ToSic.Sys.Configuration;

namespace ToSic.Eav.ImportExport.Sys.Zip;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ZipFromUrlImport(Generator<ZipImport, ZipImport.Options> zipImport, IGlobalConfiguration globalConfiguration)
    : ServiceWithSetup<ZipImport.Options>("Imp.ZipUrl", connect: [zipImport, globalConfiguration])
{

    public ZipImport.Result ImportUrl(string packageUrl, bool isAppImport, string? newName = null)
    {
        var l = Log.Fn<ZipImport.Result>($"import zip from url:'{packageUrl}', isApp:{isAppImport}, {nameof(newName)}:'{newName}'");
        var path = globalConfiguration.TemporaryFolder();
        if (path == null)
            throw new NullReferenceException("path for temporary is null - this won't work");

        var tempDirectory = new DirectoryInfo(path);
        if (!tempDirectory.Exists)
            Directory.CreateDirectory(tempDirectory.FullName);

        var destinationPath = Path.Combine(tempDirectory.FullName, Path.GetRandomFileName() + ".zip");

        using (var httpClient = new HttpClient())
        {
            try
            {
                l.A($"try to download:{packageUrl} to:{destinationPath}");
                var response = httpClient.GetAsync(packageUrl).Result; 
                response.EnsureSuccessStatusCode();

                using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                response.Content.CopyToAsync(fileStream).Wait();
            }
            catch (HttpRequestException e)
            {
                l.Ex(e);
                throw new($"Could not download app package from '{packageUrl}'.", e);
            }
        }
        ZipImport.Result success;

        var temporaryDirectory = Path.Combine(globalConfiguration.TemporaryFolder(), Guid.NewGuid().GuidCompress().Substring(0, 8));

        using (var file = File.OpenRead(destinationPath))
            success = zipImport.New(MyOptions).ImportZip(file, temporaryDirectory, newName);

        File.Delete(destinationPath);

        return l.Return(success);
    }
}