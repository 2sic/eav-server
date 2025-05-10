using System.IO;
using System.Net.Http;
using ToSic.Eav.Identity;
using ToSic.Eav.Internal.Configuration;

namespace ToSic.Eav.ImportExport.Internal.Zip;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ZipFromUrlImport: ZipImport
{
    #region DI Constructor

    public ZipFromUrlImport(MyServices services, IGlobalConfiguration globalConfiguration) : base(services)
    {
        ConnectLogs([
            _globalConfiguration = globalConfiguration
        ]);
    }

    private readonly IGlobalConfiguration _globalConfiguration;

    #endregion

    public new ZipFromUrlImport Init(int zoneId, int? appId, bool allowCode)
    {
        base.Init(zoneId, appId, allowCode);
        return this;
    }

    public bool ImportUrl(string packageUrl, bool isAppImport)
    {
        Log.A($"import zip from url:{packageUrl}, isApp:{isAppImport}");
        var path = _globalConfiguration.TemporaryFolder();
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
                Log.A($"try to download:{packageUrl} to:{destinationPath}");
                var response = httpClient.GetAsync(packageUrl).Result; 
                response.EnsureSuccessStatusCode();

                using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                response.Content.CopyToAsync(fileStream).Wait();
            }
            catch (HttpRequestException e)
            {
                Log.Ex(e);
                throw new($"Could not download app package from '{packageUrl}'.", e);
            }
        }
        bool success;

        var temporaryDirectory = Path.Combine(_globalConfiguration.TemporaryFolder(), Guid.NewGuid().GuidCompress().Substring(0, 8));

        using (var file = File.OpenRead(destinationPath))
            success = ImportZip(file, temporaryDirectory);

        File.Delete(destinationPath);

        return success;
    }
}