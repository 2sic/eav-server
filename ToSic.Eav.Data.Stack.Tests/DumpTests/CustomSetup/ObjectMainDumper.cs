using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.PropertyDump.Sys;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.Stack.DumpTests.CustomSetup;
internal class ObjectMainDumper: IPropertyDumper
{
    public const string PathToUse = "DummyPathMain";

    public const int Ranking = 50;

    public int IsCompatible(object target)
        => target is ObjectMain ? Ranking : 0;

    public List<PropertyDumpItem> Dump(object target, PropReqSpecs specs, string path, IPropertyDumpService dumpService)
    {
        var typed = (ObjectMain)target;
        if (typed == null)
            return [];

        var childProps = typed.Children
            .SelectMany(child => dumpService.Dump(child, specs, path))
            .ToList();

        return
        [
            new PropertyDumpItem
            {
                Path = PathToUse,
            },
            ..childProps,
        ];
    }
}
