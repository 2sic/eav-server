﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using ToSic.Eav.Configuration;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Eav.Serialization;
using ToSic.Eav.WebApi.Adam;
using ToSic.Eav.WebApi.Assets;
using ToSic.Eav.WebApi.Validation;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.WebApi.Sys.Licenses
{
    public class LicenseControllerReal : ServiceBase, ILicenseController
    {
        // auto-download license file
        private const string DefaultLicenseFileName = "default.license.json";

        public LicenseControllerReal(
            LazyInit<ILicenseService> licenseServiceLazy, 
            LazyInit<IFeaturesInternal> featuresLazy,
            LazyInit<IGlobalConfiguration> globalConfiguration,
            LazyInit<EavSystemLoader> systemLoaderLazy,
            LazyInit<LicenseCatalog> licenseCatalog
            ) : base("Bck.Lics")
        {
            ConnectServices(
                _licenseServiceLazy = licenseServiceLazy,
                _featuresLazy = featuresLazy,
                _globalConfiguration = globalConfiguration,
                _licenseCatalog = licenseCatalog,
                _systemLoaderLazy = systemLoaderLazy
            );
        }
        private readonly LazyInit<ILicenseService> _licenseServiceLazy;
        private readonly LazyInit<IFeaturesInternal> _featuresLazy;
        private readonly LazyInit<IGlobalConfiguration> _globalConfiguration;
        private readonly LazyInit<LicenseCatalog> _licenseCatalog;
        private readonly LazyInit<EavSystemLoader> _systemLoaderLazy;

        private string ConfigurationsPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_configurationsPath)) return _configurationsPath;
                _configurationsPath =_globalConfiguration.Value.ConfigFolder;

                // ensure that path to store files already exits
                Directory.CreateDirectory(_configurationsPath);

                return _configurationsPath;
            }
        }
        private string _configurationsPath;


        /// <inheritdoc />
        public IEnumerable<LicenseDto> Summary()
        {
            var licSer = _licenseServiceLazy.Value;
            var licenses = _licenseCatalog.Value.List.OrderBy(l => l.Priority);

            var features = _featuresLazy.Value.All;

            return licenses
                .Select(l => new LicenseDto
                {
                    Name = l.Name,
                    Priority = l.Priority,
                    Guid = l.Guid,
                    Description = l.Description,
                    AutoEnable = l.AutoEnable,
                    IsEnabled = licSer.IsEnabled(l),
                    Features = features
                        .Where(f => f.License == l.Name)
                        .OrderBy(f => f.NameId)
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
            var wrapLog = Log.Fn<LicenseFileResultDto>();

            if (!uploadInfo.HasFiles())
                return wrapLog.Return(new LicenseFileResultDto { Success = false, Message = "no file in upload" }, "no file in upload");

            var files = new List<FileUploadDto>();
            for (var i = 0; i < uploadInfo.Count; i++)
            {
                var (fileName, stream) = uploadInfo.GetStream(i);
                files.Add(new FileUploadDto { Name = fileName, Stream = stream });
            }

            foreach (var file in files) SaveLicenseFile(file);

            // reload license and features
            _systemLoaderLazy.Value.LoadLicenseAndFeatures();

            return wrapLog.ReturnAsOk(new LicenseFileResultDto { Success = true, Message = "ok" });
        }



        /// <inheritdoc />
        public LicenseFileResultDto Retrieve()
        {
            var wrapLog = Log.Fn<LicenseFileResultDto>();

            var fingerprint = _systemLoaderLazy.Value.Fingerprint.GetFingerprint();
            var url = $"https://patrons.2sxc.org/api/license/get?fingerprint={fingerprint}&version={EavSystemInfo.Version.Major}";
            Log.A($"retrieve license from url:{url}");

            string content;

            using (var client = new WebClient())
            {
                var initialProtocol = ServicePointManager.SecurityProtocol;
                try
                {
                    Log.A("Will upgrade TLS connection so we can connect with TLS 1.1 or 1.2");
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                    Log.A($"try to download:{url}");
                    content = client.DownloadString(url);

                    // verify it's json etc.
                    if (!Json.IsValidJson(content))
                        throw new ArgumentException("a file is not json");

                    // check for error
                    var licenseFileResultDto = JsonSerializer.Deserialize<LicenseFileResultDto>(content, JsonOptions.UnsafeJsonWithoutEncodingHtml);
                    if (!licenseFileResultDto.Success) 
                        return wrapLog.Return(licenseFileResultDto, licenseFileResultDto.Message);
                }
                catch (WebException e)
                {
                    Log.Ex(e);
                    throw new Exception("Could not download license file from '" + url + "'.", e);
                }
                finally
                {
                    ServicePointManager.SecurityProtocol = initialProtocol;
                }
            }

            var success = SaveLicenseFile(DefaultLicenseFileName, content);

            // reload license and features
            _systemLoaderLazy.Value.LoadLicenseAndFeatures();

            return wrapLog.ReturnAsOk(new LicenseFileResultDto { Success = success, Message = $"License file {DefaultLicenseFileName} retrieved and installed."});
        }

        private bool SaveLicenseFile(FileUploadDto file) => SaveLicenseFile(file.Name, file.Contents);

        private bool SaveLicenseFile(string fileName, string content)
        {
            var wrapLog = Log.Fn<bool>();

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
                Log.Ex(e);
                throw;
            }

            return wrapLog.ReturnTrue($"ok, save license:{filePath}");
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
}
