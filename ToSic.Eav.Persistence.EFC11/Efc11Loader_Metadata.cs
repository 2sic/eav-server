using System;
using System.Collections.Immutable;
using System.Diagnostics;
using ToSic.Eav.Persistence.Efc.Models;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader
    {
        // this is a one-time use list, it may never change during runtime!
        private static ImmutableDictionary<int, string> _metadataTypeMapPermaCache;

        private static TimeSpan InitMetadataLists(AppState app, EavDbContext dbContext)
        {
            
            var sqlTime = Stopwatch.StartNew();
            if (_metadataTypeMapPermaCache == null)
                _metadataTypeMapPermaCache = dbContext.ToSicEavAssignmentObjectTypes
                    .ToImmutableDictionary(a => a.AssignmentObjectTypeId, a => a.Name);
            sqlTime.Stop();

            app.InitMetadata(_metadataTypeMapPermaCache);

            return sqlTime.Elapsed;
        }
    }
}
