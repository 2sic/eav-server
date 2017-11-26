﻿using System.Collections.Generic;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File
{
    public class RepositoryInfoOfDemoSystem: RepositoryInfoOfFolder
    {
        public RepositoryInfoOfDemoSystem() : base(true, true, null)
        {
        }

        // this will be set from externally in various tests
        public static string PathToUse = "";

        public override List<string> RootPaths => new List<string> {PathToUse};
    }
}
