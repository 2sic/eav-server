using System.Text.RegularExpressions;

namespace ToSic.Eav.Data
{
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
                IsPage = Type == ValueConverterBase.PrefixPage;
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