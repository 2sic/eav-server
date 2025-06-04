using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.PropertyDump.Sys;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.Stack.DumpTests.CustomSetup;
internal class ObjectChildDumper : IPropertyDumper
{
    public const string PathToUse = "DummyPathChild";

    public const int Ranking = 50;

    public int IsCompatible(object target)
        => target is ObjectChild ? Ranking : 0;

    public List<PropertyDumpItem> Dump(object target, PropReqSpecs specs, string path, IPropertyDumpService dumpService)
    {
        return
        [
            new PropertyDumpItem
            {
                Path = PathToUse,
            }
        ];
    }
}
