﻿using ToSic.Eav.WebApi.Sys.Licenses;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.WebApi.Sys.Admin.Features;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class FeatureControllerReal(
    // Must be lazy, to avoid log being filled with sys-loading infos when this service is being used
    LazySvc<EavFeaturesLoader> systemLoaderLazy,
    LazySvc<ISysFeaturesService> featuresLazy)
    : ServiceBase("Bck.Feats", connect: [systemLoaderLazy, featuresLazy]), IFeatureController
{
    public const string LogSuffix = "Feats";

    public bool SaveNew(List<FeatureStateChange> changes)
    {
        var l = Log.Fn<bool>($"{changes.Count} changes");
        // validity check 
        if (changes.Count == 0)
            return l.ReturnFalse("no features changes");

        var status = systemLoaderLazy.Value.UpdateFeatures(changes);
        return l.ReturnAsOk(status);
    }

    public FeatureStateDto Details(string nameId)
    {
        var l = Log.Fn<FeatureStateDto>(nameId);
        var details = featuresLazy.Value.All.FirstOrDefault(f => f.NameId.EqualsInsensitive(nameId))
            ?? throw new NullReferenceException($"Can't find feature {nameId}");
        var dto = new FeatureStateDto(details);
        return l.ReturnAsOk(dto);
    }
}