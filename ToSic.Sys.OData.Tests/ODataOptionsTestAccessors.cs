namespace ToSic.Sys.OData.Tests;

public static class ODataOptionsTestAccessors
{
    extension(ODataOptions options)
    {
        public IReadOnlyList<string> SelectTac => options.Select;

        public string? FilterTac => options.Filter;

        public string? OrderByTac => options.OrderBy;

        public int? TopTac => options.Top;

        public int? SkipTac => options.Skip;

        public bool? CountTac => options.Count;

        public string? ExpandTac => options.Expand;

        public string? SearchTac => options.Search;

        public string? ComputeTac => options.Compute;

        public long? IndexTac => options.Index;

        public string? SkipTokenTac => options.SkipToken;

        public string? DeltaTokenTac => options.DeltaToken;

        public IReadOnlyDictionary<string, string> CustomTac => options.Custom;

        public IReadOnlyDictionary<string, string> AllRawTac => options.AllRaw;
    }
}
