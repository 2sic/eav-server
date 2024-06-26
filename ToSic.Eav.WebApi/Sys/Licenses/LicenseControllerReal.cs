﻿using System.IO;
using System.Net;
using System.Text.Json;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.Serialization;
using ToSic.Eav.SysData;
using ToSic.Eav.WebApi.Adam;
using ToSic.Eav.WebApi.Assets;
using ToSic.Eav.WebApi.Validation;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.WebApi.Sys.Licenses;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LicenseControllerReal(
    LazySvc<ILicenseService> licenseServiceLazy,
    LazySvc<IEavFeaturesService> featuresLazy,
    LazySvc<IGlobalConfiguration> globalConfiguration,
    LazySvc<EavSystemLoader> systemLoaderLazy,
    LazySvc<LicenseCatalog> licenseCatalog,
    SystemFingerprint fingerprint)
    : ServiceBase("Bck.Lics",
        connect:
        [
            licenseServiceLazy, featuresLazy, globalConfiguration, licenseCatalog, systemLoaderLazy, fingerprint
        ]), ILicenseController
{
    // auto-download license file
    private const string DefaultLicenseFileName = "default.license.json";

    private string ConfigurationsPath
    {
        get
        {
            if (!string.IsNullOrEmpty(_configurationsPath)) return _configurationsPath;
            _configurationsPath =globalConfiguration.Value.ConfigFolder;

            // ensure that path to store files already exits
            Directory.CreateDirectory(_configurationsPath);

            return _configurationsPath;
        }
    }
    private string _configurationsPath;


    /// <inheritdoc />
    public IEnumerable<LicenseDto> Summary()
    {
        var licSvc = licenseServiceLazy.Value;
        var licenses = licenseCatalog.Value.List
            .Where(l => !l.FeatureLicense)
            .OrderBy(l => l.Priority);

        var features = featuresLazy.Value.All;

        return licenses
            .Select(l =>
            {
                var state = licSvc.State(l);
                return new LicenseDto
                {
                    Name = l.Name,
                    Priority = l.Priority,
                    Guid = l.Guid,
                    Description = l.Description,
                    AutoEnable = l.AutoEnable,
                    IsEnabled = licSvc.IsEnabled(l),
                    Expires = state?.Expiration,
                    Features = features
                        .Where(f => f.License?.Name == l.Name)
                        .OrderBy(f => f.NameId)
                        .Select(f => new FeatureStateDto(f))
                        .ToList()
                };
            });
    }



    /// <summary>
    /// Not in use implementation for interface ILicenseController compatibility.
    /// Instead in use is bool Upload(HttpUploadedFile uploadInfo).
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [PrivateApi]
    public LicenseFileResultDto Upload() => throw new NotImplementedException();



    /// <summary>
    /// License-upload backend
    /// </summary>
    /// <param name="uploadInfo"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public LicenseFileResultDto Upload(HttpUploadedFile uploadInfo)
    {
        var l = Log.Fn<LicenseFileResultDto>();
        if (!uploadInfo.HasFiles())
            return l.ReturnAndLog(new() { Success = false, Message = "no file in upload" },
                "no file in upload");

        var files = new List<FileUploadDto>();
        for (var i = 0; i < uploadInfo.Count; i++)
        {
            var (fileName, stream) = uploadInfo.GetStream(i);
            files.Add(new() { Name = fileName, Stream = stream });
        }

        foreach (var file in files) SaveLicenseFile(file);

        // reload license and features
        systemLoaderLazy.Value.LoadLicenseAndFeatures();

        return l.ReturnAndLog(new() { Success = true, Message = "ok" }, "ok");
    }



    /// <inheritdoc />
    public LicenseFileResultDto Retrieve()
    {
        var l = Log.Fn<LicenseFileResultDto>();
        var fingerprint1 = fingerprint.GetFingerprint();
        var url = $"{Aspect.PatronsUrl}/api/license/get?fingerprint={fingerprint1}&version={EavSystemInfo.Version.Major}";
        l.A($"retrieve license from url:{url}");

        string content;

        using (var client = new WebClient())
        {
            var initialProtocol = ServicePointManager.SecurityProtocol;
            try
            {
                l.A("Will upgrade TLS connection so we can connect with TLS 1.1 or 1.2");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                l.A($"try to download:{url}");
                content = client.DownloadString(url);

                // verify it's json etc.
                if (!Json.IsValidJson(content))
                    throw new ArgumentException("a file is not json");

                // check for error
                var licenseFileResultDto = JsonSerializer.Deserialize<LicenseFileResultDto>(content, JsonOptions.UnsafeJsonWithoutEncodingHtml);
                if (!licenseFileResultDto.Success) 
                    return l.ReturnAndLog(licenseFileResultDto, licenseFileResultDto.Message);
            }
            catch (WebException e)
            {
                Log.Ex(e);
                throw new("Could not download license file from '" + url + "'.", e);
            }
            finally
            {
                ServicePointManager.SecurityProtocol = initialProtocol;
            }
        }

        var success = SaveLicenseFile(DefaultLicenseFileName, content);

        // reload license and features
        systemLoaderLazy.Value.LoadLicenseAndFeatures();

        return l.ReturnAndLog(new() { Success = success, Message = $"License file {DefaultLicenseFileName} retrieved and installed."}, "ok");
    }

    private bool SaveLicenseFile(FileUploadDto file) => SaveLicenseFile(file.Name, file.Contents);

    private bool SaveLicenseFile(string fileName, string content)
    {
        var l = Log.Fn<bool>();
        var filePath = Path.Combine(ConfigurationsPath, fileName);

        try
        {
            // verify it's json etc.
            if (!Json.IsValidJson(content))
                throw new ArgumentException("a file is not json");

            //  rename old file before saving new one
            if (File.Exists(filePath)) RenameOldFile(filePath);

            // save file
            File.WriteAllText(filePath, content);
        }
        catch (Exception e)
        {
            l.Ex(e);
            throw;
        }

        return l.ReturnAndLog(true, $"ok, save license:{filePath}");
    }

    private static void RenameOldFile(string filePath)
    {
        // rename old file to next free name like filename.001.bak
        var i = 0;
        var fileExists = true;
        do
        {
            i++;
            var newFileName = $"{filePath}.{i:000}.bak";
            fileExists = File.Exists(newFileName);
            if (!fileExists) File.Move(filePath, newFileName);
        } while (fileExists);
    }
}