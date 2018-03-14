using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc;

namespace ToSic.Eav.ImportExport.Tests.Mocks
{
    public class MockGlobalMetadataProvider : GlobalMetadataProvider
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
}
