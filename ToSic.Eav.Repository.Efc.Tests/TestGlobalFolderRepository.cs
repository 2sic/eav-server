﻿using System.Collections.Generic;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Repository.Efc.Tests
{
    /// <summary>
    /// Provides global information about where the folders are which should be loaded in this environment
    /// These are all the same folders as would be used at runtime on the web server
    /// </summary>
    /// <remarks>
    /// Is used by reflection, so you won't see any direct references to this anywhere
    /// </remarks>
    public class TestGlobalFolderRepository: FolderBasedRepository
    {
        public string TestRootPath = @"C:\Projects\2sxc-dnn742\Website\DesktopModules\ToSIC_SexyContent\";

        public override List<string> RootPaths => new List<string>
        {
            BuildPath(Constants.FolderData),
            //BuildPath("dist/edit/" + Constants.FolderData),
            //BuildPath("dist/sxc-edit/" + Constants.FolderData),
            BuildPath(Constants.FolderDataBeta),
            BuildPath(Constants.FolderDataCustom)
        };

        private string BuildPath(string pathEnd) => System.IO.Path.Combine(TestRootPath, pathEnd);
    }
}