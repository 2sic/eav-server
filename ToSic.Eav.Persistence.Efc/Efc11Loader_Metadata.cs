﻿using System;
using System.Diagnostics;
using ToSic.Eav.Apps;
using ToSic.Lib.Logging;
using static ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Persistence.Efc;

partial class Efc11Loader
{
    private TimeSpan InitMetadataLists(AppStateBuilder builder)
    {
        var l = Log.Fn<TimeSpan>($"{builder.AppState.Show()}");
        var sqlTime = Stopwatch.StartNew();

        builder.InitMetadata();
        sqlTime.Stop();

        return l.Return(sqlTime.Elapsed);
    }
}