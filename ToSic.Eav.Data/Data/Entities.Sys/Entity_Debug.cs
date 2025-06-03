using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.Data.PropertyLookup;

namespace ToSic.Eav.Data;

partial record Entity
{
        
    [PrivateApi]
    public List<PropertyDumpItem> _Dump(PropReqSpecs specs, string path)
        => new EntityDump().Dump(this, specs, path, null);
}