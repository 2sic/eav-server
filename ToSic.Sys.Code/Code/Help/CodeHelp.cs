using ToSic.Eav.Plumbing;
using ToSic.Sys.Utils;

namespace ToSic.Lib.Code.Help;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class CodeHelp(
    CodeHelp? original,
    string? name = default,
    string? detect = default,
    string? linkCode = default,
    bool? detectRegex = default,
    string? uiMessage = default,
    string? detailsHtml = default)
{
    public const string ErrHelpPre = "Error in your code. ";
    private const string ErrHelpLink = "https://go.2sxc.org/{0}";
    private const string ErrLinkMessage = "***** Probably {0} can help! ***** \n";
    private const string ErrHasDetails = "***** You can see more help in the toolbar. ***** \n ";
    private const string ErrHelpSuf = "What follows is the internal error: -------------------------";

    public CodeHelp(string name, string detect, string linkCode = default, bool detectRegex = default, string? uiMessage = default, string? detailsHtml = default)
        : this(original: null, name: name, detect: detect, linkCode: linkCode, detectRegex: detectRegex, uiMessage: uiMessage, detailsHtml: detailsHtml)
    { }

    /// <summary>
    /// Name for internal use to better understand what this is for
    /// </summary>
    public string? Name { get; } = name ?? original?.Name;

    public string? Detect { get; } = detect ?? original?.Detect;
    public bool DetectRegex { get; } = detectRegex ?? original?.DetectRegex ?? false;
    public string? UiMessage { get; } = uiMessage ?? original?.UiMessage;
    public string? DetailsHtml { get; } = detailsHtml ?? original?.DetailsHtml;

    public readonly string? LinkCode = linkCode ?? original?.LinkCode;

    public string? Link => LinkCode.HasValue()
        ? LinkCode.Contains("http") ? LinkCode : string.Format(ErrHelpLink, LinkCode)
        : "";

    public string LinkMessage => LinkCode.HasValue()
        ? string.Format(ErrLinkMessage, Link)
        : "";

    public string ErrorMessage => $"{ErrHelpPre} {UiMessage} {LinkMessage} {(DetailsHtml != null ? ErrHasDetails : "")} {ErrHelpSuf}";

    public override string ToString() => $"CodeHelp: {Name}";
}