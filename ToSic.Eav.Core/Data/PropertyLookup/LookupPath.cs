namespace ToSic.Eav.Data.PropertyLookup
{
    public static class LookupPath
    {
        public static string LookupPair(string source, string field) => source + "." + field;

        public static string LookupExtend(this string original, string source) => original + ">" + source;
        public static string LookupExtend(this string original, string source, string field) => original + ">" + LookupPair(source, field);


    }
}
