﻿using System.Collections.Generic;
using ToSic.Eav.Configuration;

namespace ToSic.Eav.Repositories
{
    /// <summary>
    /// Provides global information about where the folders are which should be loaded in this environment
    /// - the .data/
    /// - the dist/edit/.data/
    /// - the dist/sxc-edit/.data/
    /// - the .databeta (this is for testing only, will never be in the distribution)
    /// </summary>
    /// <remarks>
    /// Is used by reflection, so you won't see any direct references to this anywhere
    /// </remarks>
    // ReSharper disable once UnusedMember.Global
    public class GlobalFolderRepository: FolderBasedRepository
    {
        #region DI Constructor

        public GlobalFolderRepository(IGlobalConfiguration config) => _config = config;
        private readonly IGlobalConfiguration _config;

        #endregion

        public override List<string> RootPaths
        {
            get
            {
                if (_config.DataFolder == null) return new List<string>();
                //var dataRoot = DataFolder.GetDataRoot(_config.DataFolder);
                var result = new List<string>
                    {
                        _config.DataFolder,
                        _config.DataBetaFolder,
                        _config.DataCustomFolder
                        //System.IO.Path.Combine(dataRoot, ".data"),
                        //System.IO.Path.Combine(dataRoot, ".databeta"),
                        //System.IO.Path.Combine(dataRoot, ".data-custom")
                    };
                return result;
            }
        }

        //private static string GetDataRoot(string dataFolder)
        //{
        //    var systemData = Path.Combine(Constants.AppDataProtectedFolder, Constants.FolderData);
        //    if (dataFolder?.EndsWith(systemData) == true)
        //        return dataFolder.Substring(0, dataFolder.Length - systemData.Length).TrimLastSlash();
        //    if (dataFolder?.EndsWith(".data") == true)
        //        return dataFolder.Substring(0, dataFolder.Length - ".data".Length).TrimLastSlash();
        //    return dataFolder ?? string.Empty;
        //}
    }
}