using ToSic.Lib.Coding;

namespace ToSic.Eav.Apps.Internal.Insights;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class InsightsProvider(string name,
#pragma warning disable CS9113 // Parameter is unread.
    NoParamOrder protect = default,
#pragma warning restore CS9113 // Parameter is unread.
    string teaser = default,
    string helpCategory = default,
    object[] connect = default
    ): ServiceBase($"Ins.{name}", connect: connect ?? []), IInsightsProvider
{
    public const string HiddenFromAutoDisplay = "Hidden";

    public string HelpCategory => helpCategory;

    public string Name => name;

    public string Teaser => teaser;

    public virtual string Title => name;

    protected string Key { get; private set;  }

    protected bool? Toggle { get; set; }

    protected int? Position { get; private set; } 

    protected string Filter { get; private set; }
    protected string Type { get; private set; }
    protected string NameId { get; private set; }

    public void SetContext(IInsightsLinker linker, int? appId, IDictionary<string, object> parameters, string key, int? position, string type, bool? toggle, string nameId, string filter)
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
    protected IInsightsLinker Linker { get; private set; }
    protected int? AppId { get; private set; }
    protected IDictionary<string, object> Parameters { get; private set; }


    public abstract string HtmlBody();

    #region Url Params Checkers

    protected bool UrlParamsIncomplete(int? appId, out string message)
        => UrlParamIncomplete("appid", appId, out message);


    protected bool UrlParamsIncomplete(int? appId, string type, out string message)
        => UrlParamsIncomplete(appId, out message)
           || UrlParamIncomplete("type", type, out message);


    protected bool UrlParamsIncomplete(int? appId, string type, string attribute, out string message)
        => UrlParamsIncomplete(appId, type, out message)
           || UrlParamIncomplete("attribute", attribute, out message);

    protected bool UrlParamsIncomplete(int? appId, int? entity, out string message)
        => UrlParamsIncomplete(appId, out message)
           || UrlParamIncomplete("entity", entity, out message);

    /// <summary>
    /// verify hat a value is not null, otherwise give a reasonable message back
    /// </summary>
    /// <param name="name">parameter name for the message</param>
    /// <param name="value">value object which is expected to be not null</param>
    /// <param name="message">returned message</param>
    /// <returns>true if incomplete, false if ok</returns>
    private static bool UrlParamIncomplete(string name, object value, out string message)
    {
        message = null;
        if (value != null) return false;
        message = $"please add '{name}' to the url parameters";
        return true;
    }

    #endregion
}