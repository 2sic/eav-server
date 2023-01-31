using System;
using System.Collections.Immutable;
using System.Diagnostics;
using ToSic.Eav.Apps;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Lib.Logging;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader
    {
        // #removeUnusedPreloadOfMetaTypes
        //// this is a one-time use list, it may never change during runtime!
        //private static ImmutableDictionary<int, string> _metadataTypeMapPermaCache;

        private TimeSpan InitMetadataLists(AppState app/*, EavDbContext dbContext*/)
        {
            var l = Log.Fn<TimeSpan>($"{(app as IAppIdentity).Show()}");
            var sqlTime = Stopwatch.StartNew();
            // #removeUnusedPreloadOfMetaTypes
            //if (_metadataTypeMapPermaCache == null)
            //    _metadataTypeMapPermaCache = dbContext.ToSicEavAssignmentObjectTypes
            //        .ToImmutableDictionary(a => a.AssignmentObjectTypeId, a => a.Name);

            app.InitMetadata(/*_metadataTypeMapPermaCache*/);
            sqlTime.Stop();

            return l.Return(sqlTime.Elapsed);
        }
    }
}
