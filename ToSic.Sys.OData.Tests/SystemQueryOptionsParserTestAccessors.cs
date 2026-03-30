namespace ToSic.Sys.OData.Tests;

internal static class SystemQueryOptionsParserTestAccessors
{
    internal static ODataOptions ParseTac(this Uri uri)
        => SystemQueryOptionsParser.Parse(uri);
}
