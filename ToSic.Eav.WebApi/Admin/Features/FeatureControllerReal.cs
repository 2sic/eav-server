﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Plumbing;
using ToSic.Eav.SysData;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.WebApi.Admin.Features;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class FeatureControllerReal : ServiceBase, IFeatureController
{
    /// <summary>
    /// Must be lazy, to avoid log being filled with sys-loading infos when this service is being used
    /// </summary>
    private readonly LazySvc<EavSystemLoader> _systemLoaderLazy;
    private readonly LazySvc<IEavFeaturesService> _featuresLazy;
    public const string LogSuffix = "Feats";

    #region Constructor / DI

    public FeatureControllerReal(
        LazySvc<EavSystemLoader> systemLoaderLazy,
        LazySvc<IEavFeaturesService> featuresLazy
    ) : base("Bck.Feats")
    {
        ConnectServices(
            _systemLoaderLazy = systemLoaderLazy,
            _featuresLazy = featuresLazy
        );
    }



    #endregion

    public bool SaveNew(List<FeatureManagementChange> changes)
    {
        var l = Log.Fn<bool>($"{changes.Count} changes");
        // validity check 
        if (changes == null || changes.Count == 0)
            return l.ReturnFalse("no features changes");

        return l.ReturnAsOk(_systemLoaderLazy.Value.UpdateFeatures(changes));
    }

    public FeatureState Details(string nameId)
    {
        var l = Log.Fn<FeatureState>(nameId);
        var details = _featuresLazy.Value.All.FirstOrDefault(f => f.NameId.EqualsInsensitive(nameId));
        return l.ReturnAsOk(details);
    }
}