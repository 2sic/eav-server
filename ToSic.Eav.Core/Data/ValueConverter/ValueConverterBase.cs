using System;
using System.Text.RegularExpressions;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Trivial value converter - doesn't convert anything.
    /// </summary>
    public class ValueConverterBase : IValueConverter
    {
        public const string PrefixPage = "page";
        public const string PrefixFile = "file";
        public const string Separator = ":";


        public string ToReference(string value) => value;

        public string ToValue(string reference, Guid itemGuid) => reference;

        public static bool CouldBeReference(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) return false;
            if (!reference.Contains(Separator)) return false;
            if (reference.Length < 6) return false; // minimum "page|file:number"
            return true;
        }
        
        public class LinkParts
        {
            public LinkParts(string link)
            {
                var regularExpression = Regex.Match(link, RegExToDetectConvertable, RegexOptions.IgnoreCase);
                IsMatch = regularExpression.Success;
                if (IsMatch)
                {
                    Type = regularExpression.Groups[RegExType].Value.ToLowerInvariant();
                    Id = int.Parse(regularExpression.Groups[RegExId].Value);
                    Params = regularExpression.Groups[RegExParams].Value ?? "";
                    IsPage = Type == PrefixPage;
                }

            }

            public readonly bool IsMatch;
            public readonly bool IsPage;
            public readonly string Type;
            public readonly int Id;
            public readonly string Params;

            public const string RegExToDetectConvertable = @"^(?<type>(file|page)):(?<id>[0-9]+)(?<params>(\?|\#).*)?$";
            public const string RegExType = "type";
            public const string RegExId = "id";
            public const string RegExParams = "params";
        }
    }
}
