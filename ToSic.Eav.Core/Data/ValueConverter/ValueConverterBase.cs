using System;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Trivial value converter - doesn't convert anything.
    /// </summary>
    public partial class ValueConverterBase : IValueConverter
    {
        public const string PrefixPage = "page";
        public const string PrefixFile = "file";
        public const string Separator = ":";


        public string ToReference(string value) => value;

        public string ToValue(string reference, Guid itemGuid) => reference;

        public static bool CouldBeReference(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) return false;
            // must contain ":"
            if (!reference.Contains(Separator)) return false;
            // Avoid false positives on full paths
            if (reference.Contains("/") || reference.Contains("\\")) return false;
            // minimum "page:#" or "file:#"
            if (reference.Length < 6) return false; 
            return true;
        }

        public static string TryToResolveCodeToLink(Guid itemGuid, string originalValue, Func<int, string> resolvePageLink, Func<int, Guid, string> resolveFileLink)
        {
            if (string.IsNullOrEmpty(originalValue)) return originalValue;

            // new
            var resultString = originalValue;

            var parts = new LinkParts(resultString);

            // var regularExpression = Regex.Match(resultString, ValueConverterBase.RegExToDetectConvertable, RegexOptions.IgnoreCase);

            if (!parts.IsMatch) // regularExpression.Success)
                return originalValue;

            //var linkType = regularExpression.Groups[ValueConverterBase.RegExType].Value.ToLowerInvariant();
            //var linkId = int.Parse(regularExpression.Groups[ValueConverterBase.RegExId].Value);
            //var urlParams = regularExpression.Groups[ValueConverterBase.RegExParams].Value ?? "";

            //var isPageLookup = linkType == ValueConverterBase.PrefixPage;

            var result = (parts.IsPage // isPageLookup
                             ? resolvePageLink(parts.Id)
                             : resolveFileLink(parts.Id, itemGuid))
                         ?? originalValue;

            return result + (result == originalValue ? "" : parts.Params);
        }
    }
}
