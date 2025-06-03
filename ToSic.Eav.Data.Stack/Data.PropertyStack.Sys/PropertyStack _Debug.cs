using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.PropertyLookup;

namespace ToSic.Eav.Data;

partial class PropertyStack
{
    [PrivateApi("Internal")]
    public List<PropertyDumpItem> _Dump(PropReqSpecs specs, string path)
        => new PropertyStackDump().Dump(this, specs, path, null);
}