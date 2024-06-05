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

    protected string Key { get; private set;  } // => Parameters.TryGetValue("key", out var key) ? key as string : null;

    protected bool? Toggle { get; set; } //=> Parameters.TryGetValue("toggle", out var toggle) && toggle is true;

    protected int? Position { get; private set; } 

    protected string Filter { get; private set; }

    public void SetContext(IInsightsLinker linker, int? appId, IDictionary<string, object> parameters, string key, int? position, string type, bool? toggle, string nameId, string filter)
    {
        Linker = linker;
        AppId = appId;
        Parameters = parameters;
        Key = key;
        Toggle = toggle;
        Position = position;
        Filter = filter;
    }
    protected IInsightsLinker Linker { get; private set; }
    protected int? AppId { get; private set; }
    protected IDictionary<string, object> Parameters { get; private set; }


    public abstract string HtmlBody();
}