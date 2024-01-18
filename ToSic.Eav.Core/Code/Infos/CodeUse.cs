namespace ToSic.Eav.Code.Infos;

/// <summary>
/// Information that a code was used - must always contain a code-info which describes what was used.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class CodeUse(ICodeInfo change, int appId = default, string specificId = default, string[] more = default)
{
    public ICodeInfo Change { get; } = change;
    public int AppId { get; } = appId; // FYI: ATM not reported anywhere
    public string SpecificId { get; } = specificId;
    public string[] More { get; } = more;


    public CodeUse UsedAs(int appId = default, string specificId = default, string[] more = default)
        => new(Change,
            appId: appId == default ? AppId : appId,
            specificId: specificId ?? SpecificId,
            more: more ?? More);

}