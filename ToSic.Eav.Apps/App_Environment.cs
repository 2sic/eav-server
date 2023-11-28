﻿using System.IO;
using ToSic.Eav.Context;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps;

partial class App
{
    /// <inheritdoc />
    [PrivateApi]
    public ISite Site { get; protected set; }


    #region Paths

    [PrivateApi]
    public string PhysicalPath => AppStateInt.GetPiggyBack(nameof(PhysicalPath), () => Path.Combine(Site.AppsRootPhysicalFull, Folder));

    #endregion

}