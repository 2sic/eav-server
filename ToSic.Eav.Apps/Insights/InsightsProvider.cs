using System.Collections.Generic;
using ToSic.Lib.Coding;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Insights;

public abstract class InsightsProvider(string name, NoParamOrder protect = default, string teaser = default, string helpCategory = default): ServiceBase($"Ins.{name}"), IInsightsProvider
{
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