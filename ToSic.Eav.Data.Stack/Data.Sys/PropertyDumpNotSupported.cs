using ToSic.Eav.Data.Sys.PropertyDump;

namespace ToSic.Eav.Data.Sys;
public class PropertyDumpNotSupportedFallback: IPropertyDumper
{
    public int IsCompatible(object target) => 1;

    public List<PropertyDumpItem> Dump(object target, PropReqSpecs specs, string path, IPropertyDumpService dumpService)
        => [new() { Path = $"Not supported on '{target?.GetType().Name}'" }];
}
