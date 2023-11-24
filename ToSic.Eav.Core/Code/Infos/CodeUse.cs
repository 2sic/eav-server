namespace ToSic.Eav.Code.Infos;

/// <summary>
/// Information that a code was used - must always contain a code-info which describes what was used.
/// </summary>
public class CodeUse
{
    public CodeUse(ICodeInfo change, int appId = default, string specificId = default, string[] more = default)
    {
        Change = change;
        SpecificId = specificId;
        More = more;
        AppId = appId;
    }

    public ICodeInfo Change { get; }
    public int AppId { get; }   // FYI: ATM not reported anywhere
    public string SpecificId { get; }
    public string[] More { get; }


    public CodeUse UsedAs(int appId = default, string specificId = default, string[] more = default)
        => new(Change,
            appId: appId == default ? AppId : appId,
            specificId: specificId ?? SpecificId,
            more: more ?? More);

}