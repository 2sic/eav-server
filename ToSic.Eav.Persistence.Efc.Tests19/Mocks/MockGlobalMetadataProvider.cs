using System.Collections.Immutable;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc.Tests19.Mocks;

public class MockGlobalMetadataProvider(LazySvc<EavDbContext> dbLazy) : EfcMetadataTargetTypes(dbLazy)
{        
    protected override ImmutableDictionary<int, string> GetTargetTypes()
    {
        return (new Dictionary<int, string>
        {
            {1, "Default" },
            {2, "EAV Field Properties" },
            {3, "App" },
            {4, "Entity" },
            {5, "ContentType" },
            {6, "Zone" },
            {7, "Reserved" },
            {8, "Reserved" },
            {9, "Reserved" },
            {10, "CmsObject" },
        }).ToImmutableDictionary();
    }
}