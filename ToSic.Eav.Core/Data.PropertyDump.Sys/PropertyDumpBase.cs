using ToSic.Eav.Data.PropertyLookup;

namespace ToSic.Eav.Data.PropertyDump.Sys;
public class PropertyDumpBase
{
    public void SetupVoid(PropReqSpecs specs, string path)
    {
        Specs = specs ?? throw new ArgumentNullException(nameof(specs), "Specs must not be null");
        Path = path ?? throw new ArgumentNullException(nameof(path), "Path must not be null");
    }

    public PropReqSpecs Specs;
    public string Path;
}

public static class PropertyDumpBaseExtensions
{
    public static TPropertyDump Setup<TPropertyDump>(this TPropertyDump dumpBase, PropReqSpecs specs, string path) where TPropertyDump : PropertyDumpBase
    {
        dumpBase.SetupVoid(specs, path);
        return dumpBase;
    }
}
