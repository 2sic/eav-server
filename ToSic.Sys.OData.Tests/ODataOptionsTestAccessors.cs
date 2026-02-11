namespace ToSic.Sys.OData.Tests;

public static class ODataOptionsTestAccessors
{
    extension(ODataOptions options)
    {
        public IReadOnlyDictionary<string, string> CustomTac => options.Custom;

        public IReadOnlyDictionary<string, string> AllRawTac => options.AllRaw;
    }
}
