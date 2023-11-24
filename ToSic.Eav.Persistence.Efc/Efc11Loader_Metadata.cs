using System;
using System.Diagnostics;
using ToSic.Eav.Apps;
using ToSic.Lib.Logging;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Persistence.Efc;

partial class Efc11Loader
{
    private TimeSpan InitMetadataLists(AppState app)
    {
        var l = Log.Fn<TimeSpan>($"{(app as IAppIdentity).Show()}");
        var sqlTime = Stopwatch.StartNew();

        app.InitMetadata();
        sqlTime.Stop();

        return l.Return(sqlTime.Elapsed);
    }
}