﻿using System.Text.Json;
using ToSic.Eav.Data.Entities.Sys.Lists;
using ToSic.Eav.Data.Global.Sys;
using ToSic.Eav.Serialization.Sys.Json;
using ToSic.Eav.Sys;
using ToSic.Sys.Capabilities.Licenses;
using SysFeaturesService = ToSic.Sys.Capabilities.SysFeatures.SysFeaturesService;

namespace ToSic.Sys.Capabilities.Features;

[PrivateApi]
public class EavFeaturesLoader(
    ISysFeaturesService featuresSvc,
    FeaturePersistenceService featurePersistenceService,
    FeaturesIoHelper featuresIo,
    LicenseLoader licenseLoader,
    IGlobalDataService globalData,
    SysFeaturesService sysFeaturesService)
    : ServiceBase($"{EavLogs.Eav}FtLdr",
        connect: [globalData, featuresSvc, featurePersistenceService, featuresIo, licenseLoader, sysFeaturesService])
{

    /// <summary>
    /// Standalone Features loading - to make the features API available in tests
    /// </summary>
    public void LoadLicenseAndFeatures()
    {
        var l = Log.Fn();
        try
        {
            var list = globalData.ListRequired;
            l.A($"list:{list.Count}");

            var licEntities = list
                .OfType(LicenseEntity.TypeNameId)
                .Select(e => new LicenseEntity(e))
                .ToListOpt();
            l.A($"licEnt:{licEntities.Count}");

            // Check all licenses and show extra message
            var enterpriseLicenses = licEntities
                .Select(lic =>
                {
                    var entFp = lic.AsEnterprise();
                    l.A(entFp.ValidityMessage);
                    return entFp;
                })
                .ToListOpt();
            l.A($"entLic:{enterpriseLicenses.Count}");

            licenseLoader.Init(enterpriseLicenses);
            licenseLoader.LoadLicenses();
        }
        catch (Exception e)
        {
            l.Ex(e);
            l.Done("error");
            return;
        }

        // Now do a normal reload of configuration and features
        ReloadFeatures();
        l.Done("ok");
    }

    /// <summary>
    /// Reset the features stored by loading from 'features.json'.
    /// </summary>
    [PrivateApi]
    internal bool ReloadFeatures()
    {
        var l = Log.Fn<bool>();
        var stored = LoadFeaturesStored() ?? new FeatureStatesPersisted();
        var status = featuresSvc.UpdateFeatureList(stored, sysFeaturesService.States);

        // Trigger any necessary feature state update code
        foreach (var featureState in featuresSvc.All)
            featureState.Feature.RunOnStateChange?.Invoke(featureState, l);

        return l.ReturnAndLog(status);
    }

    /// <summary>
    /// Load features stored from 'features.json'.
    /// When old format is detected, it is converted to new format.
    /// </summary>
    /// <returns></returns>
    internal FeatureStatesPersisted? LoadFeaturesStored()
    {
        var l = Log.Fn<FeatureStatesPersisted?>();
        try
        {
            var (_, fileContent) = featuresIo.Load();
            if (string.IsNullOrEmpty(fileContent)) 
                return l.ReturnNull("ok, but 'features.json' is missing");

            // return features stored
            var newState = JsonSerializer.Deserialize<FeatureStatesPersisted>(fileContent, JsonOptions.UnsafeJsonWithoutEncodingHtml);

            return l.Return(newState, "ok, features loaded");
        }
        catch (Exception e)
        {
            l.Ex(e);
            return l.ReturnNull("load feature failed:" + e.Message);
        }
    }


    /// <summary>
    /// Update existing features config in "features.json". 
    /// </summary>
    [PrivateApi]
    public bool UpdateFeatures(List<FeatureStateChange> changes)
    {
        var l = Log.Fn<bool>($"c:{changes?.Count ?? -1}");
        var saved = featurePersistenceService.ApplyUpdatesAndSave(changes);
        var ok = ReloadFeatures();
        return l.ReturnAndLog(saved && ok, "ok, updated");
    }

}