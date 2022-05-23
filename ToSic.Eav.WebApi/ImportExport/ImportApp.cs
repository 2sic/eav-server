﻿using System;
using System.IO;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Identity;
using ToSic.Eav.Logging;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.ImportExport
{
    public class ImportApp: HasLog<ImportApp>
    {
        #region DI Constructor

        public ImportApp(IEnvironmentLogger envLogger, ZipImport zipImport, IGlobalConfiguration globalConfiguration, IUser user) : base("Bck.Export")
        {
            _envLogger = envLogger;
            _zipImport = zipImport;
            _globalConfiguration = globalConfiguration;
            _user = user;
        }

        private readonly IEnvironmentLogger _envLogger;
        private readonly ZipImport _zipImport;
        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly IUser _user;

        #endregion

        public ImportResultDto Import(Stream stream, int zoneId, string renameApp)
        {
            Log.A("import app start");
            var result = new ImportResultDto();

            if (!string.IsNullOrEmpty(renameApp)) Log.A($"new app name: {renameApp}");

            var zipImport = _zipImport;
            try
            {
                zipImport.Init(zoneId, null, _user.IsSuperUser, Log);
                var temporaryDirectory = Path.Combine(_globalConfiguration.TemporaryFolder, Mapper.GuidCompress(Guid.NewGuid()).Substring(0, 8));

                // Increase script timeout to prevent timeouts
                result.Success = zipImport.ImportZip(stream, temporaryDirectory, renameApp);
                result.Messages.AddRange(zipImport.Messages);
            }
            catch (Exception ex)
            {
                _envLogger.LogException(ex);
                result.Success = false;
                result.Messages.AddRange(zipImport.Messages);
            }
            return result;
        }

    }
}
