using System.Collections.Generic;
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
                var result = new List<string>
                    {
                        _config.DataFolder,
                        _config.DataBetaFolder,
                        _config.DataCustomFolder
                    };
                return result;
            }
        }
    }
}