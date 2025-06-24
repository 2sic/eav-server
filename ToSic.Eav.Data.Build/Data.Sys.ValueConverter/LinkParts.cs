using System.Text.RegularExpressions;

namespace ToSic.Eav.Data.ValueConverter.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class LinkParts
{
    public LinkParts(string link)
    {
        var regularExpression = Regex.Match(link, RegExToDetectConvertible, RegexOptions.IgnoreCase);
        IsMatch = regularExpression.Success;
        if (!IsMatch)
            return;

        Type = regularExpression.Groups[RegExType].Value.ToLowerInvariant();
        Id = int.Parse(regularExpression.Groups[RegExId].Value);
        Params = regularExpression.Groups[RegExParams].Value;
        IsPage = Type == ValueConverterBase.PrefixPage;

    }

    /// <summary>
    /// This is a special temp overload to detect `file:filename.jpg` variations
    /// </summary>
    /// <param name="link"></param>
    /// <param name="someOtherParameter"></param>
    public LinkParts(string link, bool someOtherParameter)
    {
        var regularExpression = Regex.Match(link, RegExToDetectName, RegexOptions.IgnoreCase);
        IsMatch = regularExpression.Success;
        if (!IsMatch)
            return;

        Type = regularExpression.Groups[RegExType].Value.ToLowerInvariant();
        Name = regularExpression.Groups[RegExName].Value;
        Params = regularExpression.Groups[RegExParams].Value ?? "";
        IsPage = Type == ValueConverterBase.PrefixPage;
    }

    public readonly bool IsMatch;
    public readonly bool IsPage;
    public readonly string? Type;
    public readonly int Id;

    /// <summary>
    /// File name, can be null if using the file:123 format instead of file:name.jpg
    /// </summary>
    public readonly string? Name;
    public bool UseName => Name != null;
    public readonly string? Params;

    // language=regex
    private const string RegExToDetectConvertible = @"^(?<type>(file|page)):(?<id>[0-9]+)(?<params>(\?|\#).*)?$";
    // language=regex
    private const string RegExToDetectName = @"^(?<type>(file|adam)):(?<name>[^\\^\/^?]+)(?<params>(\?|\#).*)?$";
    private const string RegExType = "type";
    private const string RegExId = "id";
    private const string RegExName = "name";
    private const string RegExParams = "params";

    #region Static Helpers

    public static int? CheckIdStringForId(string? id)
    {
        if (!id.HasValue())
            return null;
        var linkParts = new LinkParts(id);
        if (!linkParts.IsMatch || linkParts.Id == 0)
            return null;
        return linkParts.Id;
    }

    #endregion
}