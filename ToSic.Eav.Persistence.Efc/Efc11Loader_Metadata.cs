﻿using System.Diagnostics;

namespace ToSic.Eav.Persistence.Efc;

partial class Efc11Loader
{
    private TimeSpan InitMetadataLists(IAppStateBuilder builder)
    {
        var l = Log.Fn<TimeSpan>($"{builder.AppState.Show()}");
        var sqlTime = Stopwatch.StartNew();

        builder.InitMetadata();
        sqlTime.Stop();

        return l.Return(sqlTime.Elapsed);
    }
}