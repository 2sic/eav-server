namespace ToSic.Lib;

/// <summary>
/// Contains information for all assemblies to use
/// </summary>
public static class LibAssemblyInfo
{
    // Important: These must be constants!
    // This is because the version etc. may be compiled into Oqtane DLLs
    // which will run in the browser, and shouldn't have to include these DLLs to work
    // If it's a constant, the value will be added to the compiled code, so no real dependency will exist at runtime
    public const string AssemblyVersion = "19.99.00";
    public const string Company = "2sic internet solutions GmbH, Switzerland";
    public const string EavProduct = "2sic Lib Foundation";
    public const string EavCopyright = "Copyright MIT © 2sic 2025";
}