namespace ToSic.Lib.Code.Infos;

/// <summary>
/// Information about the current code - such as obsolete-message or important notification
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class CodeInfo : ICodeInfo
{
    protected CodeInfo(CodeInfoTypes type, string nameId, Version from, Version to, string? link = default, string? message = default)
    {
        NameId = nameId;
        Link = link;
        Message = message;
        From = from;
        To = to;
        Type = type;
    }

    private CodeInfo(ICodeInfo original, string? nameId = default, string? link = default, string? message = default, CodeInfoTypes? type = default)
    {
        NameId = nameId ?? original.NameId;
        Link = link ?? original.Link;
        Message = message ?? original.Message;
        From = original.From;
        To = original.To;
        Type = type ?? original.Type;
    }


    public string NameId { get; }

    public Version From { get; }

    public Version To { get; }

    public string? Link { get; }

    public string? Message { get; }

    public CodeInfoTypes Type { get; }

    /// <summary>
    /// Create a new CodeChangeInfo with placeholders like {0} replaced.
    /// </summary>
    /// <param name="replacements"></param>
    /// <returns></returns>
    public ICodeInfo Replace(params object[] replacements) => new CodeInfo(this,
        nameId: string.Format(NameId, replacements),
        message: string.Format(Message ?? "", replacements));

    public CodeUse UsedAs(int appId = default, string? specificId = default, string[]? more = default) =>
        new(this, appId: appId, specificId: specificId, more: more);
}