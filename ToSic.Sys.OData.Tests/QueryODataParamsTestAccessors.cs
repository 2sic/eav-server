namespace ToSic.Sys.OData.Tests;

internal static class QueryODataParamsTestAccessors
{
    internal static ODataOptions CreateTac(
        Func<IDictionary<string, string>, IDictionary<string, string>> parseFunc,
        string? streamName = default)
        => QueryODataParams.Create(parseFunc, streamName);

    internal static Dictionary<string, ODataOptions> CreateManyTac(
        Func<IDictionary<string, string>, IDictionary<string, string>> parseFunc,
        string[] streamNames,
        string? selectedStream = default)
        => QueryODataParams.CreateMany(parseFunc, streamNames, selectedStream);
}
