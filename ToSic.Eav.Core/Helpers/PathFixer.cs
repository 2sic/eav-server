namespace ToSic.Eav.Helpers
{
    public static class PathFixer
    {
        /// <summary>
        /// Convert all "/" characters to "\" characters - usually to change url-style paths to folder style paths
        /// </summary>
        /// <returns></returns>
        public static string Backslash(this string original)
            => original?.Replace("/", "\\");
        // Additional replace too risky, breaks network paths like \\srv-xyz\
        // .Replace("\\\\", "\\");


        /// <summary>
        /// Convert all "/" characters to "\" characters
        /// </summary>
        public static string Forwardslash(this string original)
            => original?.Replace("\\", "/");
            // could break https:// links etc.
            // .Replace("//", "/").Replace("//", "/");

        public static string PrefixSlash(this string original)
        {
            if (original == null) return "/";
            if (original.StartsWith("/")) return original;
            if (original.StartsWith("\\")) original = original.TrimStart('\\');
            return "/" + original;
        }

        public static string TrimLastSlash(this string original)
            => original?.TrimEnd('/').TrimEnd('\\');

        public static string TrimPrefixSlash(this string original)
            => original?.TrimStart('/').TrimStart('\\');
    }
}
