namespace ToSic.Eav.Sys.Insights;

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class InsightsProvider(
    InsightsProviderSpecs specs,
#pragma warning disable CS9113 // Parameter is unread.
    NoParamOrder npo = default,
#pragma warning restore CS9113 // Parameter is unread.
    object[]? connect = default
): ServiceBase($"Ins.{specs.Name}", connect: connect ?? []),
    IInsightsProvider
{
    public InsightsProviderSpecs Specs => specs; 

    protected string? Key { get; private set;  }

    protected bool? Toggle { get; set; }

    protected int? Position { get; private set; } 

    protected string? Filter { get; private set; }
    protected string? Type { get; private set; }
    protected string? NameId { get; private set; }

    public void SetContext(IInsightsLinker linker, int? appId, IDictionary<string, object?> parameters, string key, int? position, string type, bool? toggle, string nameId, string filter)
    {
        Linker = linker;
        AppId = appId;
        Parameters = parameters;
        Key = key;
        Toggle = toggle;
        Position = position;
        Filter = filter;
        Type = type;
        NameId = nameId;
    }
    protected IInsightsLinker Linker { get; private set; } = null!;
    protected int? AppId { get; private set; }
    protected IDictionary<string, object?> Parameters { get; private set; } = null!;


    public abstract string HtmlBody();

    #region Url Params Checkers

    protected bool UrlParamsIncomplete([NotNullWhen(false)] int? appId, [NotNullWhen(true)] out string? message)
        => UrlParamIncomplete("appid", appId, out message);


    protected bool UrlParamsIncomplete([NotNullWhen(false)] int? appId, [NotNullWhen(false)] string? type, [NotNullWhen(true)] out string? message)
        => UrlParamsIncomplete(appId, out message)
           || UrlParamIncomplete("type", type, out message);


    protected bool UrlParamsIncomplete([NotNullWhen(false)] int? appId, [NotNullWhen(false)] string? type, [NotNullWhen(false)] string? attribute, [NotNullWhen(true)] out string? message)
        => UrlParamsIncomplete(appId, type, out message)
           || UrlParamIncomplete("attribute", attribute, out message);

    protected bool UrlParamsIncomplete([NotNullWhen(false)] int? appId, [NotNullWhen(false)] int? entity, [NotNullWhen(true)] out string? message)
        => UrlParamsIncomplete(appId, out message)
           || UrlParamIncomplete("entity", entity, out message);

    /// <summary>
    /// verify hat a value is not null, otherwise give a reasonable message back
    /// </summary>
    /// <param name="name">parameter name for the message</param>
    /// <param name="value">value object which is expected to be not null</param>
    /// <param name="message">returned message</param>
    /// <returns>true if incomplete, false if ok</returns>
    private static bool UrlParamIncomplete(string name, object? value, [NotNullWhen(true)] out string? message)
    {
        message = null;
        if (value != null)
            return false;
        message = $"please add '{name}' to the url parameters";
        return true;
    }

    #endregion
}