namespace ToSic.Eav.LookUp;

public interface ITweakLookUp
{
    ITweakLookUp PostProcess(Func<string, string> postProcess);
    ITweakLookUp PostProcess(Func<string, LookUpSpecs, string> postProcessAdv);
    string PostProcess(string template, LookUpSpecs specs);
}