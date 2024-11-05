namespace ToSic.Eav.LookUp;

/// <summary>
/// Tweak lookup.
/// Important: not fully implemented yet, so not fully functional as most tweaks, currently a quick-hack
/// to get Html Lookups working.
/// </summary>
internal class TweakLookUp: ITweakLookUp
{
    internal TweakLookUp()
    { }

    internal TweakLookUp(
        TweakLookUp original,
        Func<string, string> postProcess = default,
        Func<string, LookUpSpecs, string> postProcessAdv = default
    )
    {
        _postProcess = postProcess ?? original._postProcess;
        _postProcessAdv = postProcessAdv ?? original._postProcessAdv;
    }

    public ITweakLookUp PostProcess(Func<string, string> postProcess)
        => new TweakLookUp(this, postProcess: postProcess);

    public ITweakLookUp PostProcess(Func<string, LookUpSpecs, string> postProcessAdv)
        => new TweakLookUp(this, postProcessAdv: postProcessAdv);

    private readonly Func<string, string> _postProcess;
    private readonly Func<string, LookUpSpecs, string> _postProcessAdv;

    internal string PostProcess(string template, LookUpSpecs specs)
    {
        return _postProcess != null
            ? _postProcess(template)
            : _postProcessAdv != null
                ? _postProcessAdv(template, specs)
                : template;
    }
}