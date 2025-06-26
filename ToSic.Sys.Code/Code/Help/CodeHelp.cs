namespace ToSic.Sys.Code.Help;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record CodeHelp
{
    public const string ErrHelpPre = "Error in your code. ";
    private const string ErrHelpLink = "https://go.2sxc.org/{0}";
    private const string ErrLinkMessage = "***** Probably {0} can help! ***** \n";
    private const string ErrHasDetails = "***** You can see more help in the toolbar. ***** \n ";
    private const string ErrHelpSuf = "What follows is the internal error: -------------------------";

    /// <summary>
    /// Name for internal use to better understand what this is for. Can be anything, it's just to self-document what the help is about.
    /// </summary>
    public required string? Name { get; init; }

    public required string? Detect { get; init; }
    public bool DetectRegex { get; init; }
    public string? UiMessage { get; init; }
    public string? DetailsHtml { get; init; }

    public string? LinkCode { get; init; }

    public string? Link
        => LinkCode.HasValue()
            ? LinkCode.Contains("http") ? LinkCode : string.Format(ErrHelpLink, LinkCode)
            : "";

    public string LinkMessage
        => LinkCode.HasValue()
            ? string.Format(ErrLinkMessage, Link)
            : "";

    public string ErrorMessage
        => $"{ErrHelpPre} {UiMessage} {LinkMessage} {(DetailsHtml != null ? ErrHasDetails : "")} {ErrHelpSuf}";

    public override string ToString()
        => $"CodeHelp: {Name}";
}