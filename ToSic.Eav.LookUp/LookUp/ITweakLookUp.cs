﻿using ToSic.Eav.LookUp.Sys;

namespace ToSic.Eav.LookUp;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface ITweakLookUp
{
    ITweakLookUp PostProcess(Func<string, string> postProcess);
    ITweakLookUp PostProcess(Func<string, LookUpSpecs, string> postProcessAdv);
    string PostProcess(string template, LookUpSpecs specs);
}