using ToSic.Lib.Coding;

namespace ToSic.Eav.Apps.Internal.Insights;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class InsightsProvider(string name,
#pragma warning disable CS9113 // Parameter is unread.
    NoParamOrder protect = default,
#pragma warning restore CS9113 // Parameter is unread.
    string teaser = default,
    string helpCategory = default
    ): ServiceBase($"Ins.{name}"), IInsightsProvider
{
    public const string HiddenFromAutoDisplay = "Hidden";

    public string HelpCategory => helpCategory;

    public string Name => name;

    public string Teaser => teaser;

    public virtual string Title => name;

    public void SetContext(IInsightsLinker linker, int? appId = default, IDictionary<string, object> parameters = default)
    {
        Linker = linker;
        AppId = appId;
        Parameters = parameters;
    }
    protected IInsightsLinker Linker { get; private set; }
    protected int? AppId { get; private set; }
    protected IDictionary<string, object> Parameters { get; private set; }


    public abstract string HtmlBody();
}