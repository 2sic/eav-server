using System.Text.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.Serialization;
using ToSic.Eav.SysData;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Loaders;

[PrivateApi]
public class EavFeaturesLoader(
    SystemFingerprint fingerprint,  // note: must be of type SystemFingerprint, not IFingerprint
    IEavFeaturesService featuresSvc,
    FeaturePersistenceService featurePersistenceService,
    LicenseLoader licenseLoader,
    IAppReaderFactory appReaders,
    SysFeaturesService sysFeaturesService)
    : ServiceBase($"{EavLogs.Eav}FtLdr",
        connect: [fingerprint, appReaders, featuresSvc, featurePersistenceService, licenseLoader, sysFeaturesService])
{

    /// <summary>
    /// Standalone Features loading - to make the features API available in tests
    /// </summary>
    public void LoadLicenseAndFeatures()
    {
        var l = Log.Fn();
        try
        {
            var presetApp = appReaders.GetSystemPreset();
            l.A($"presetApp:{presetApp != null}");

            var licEntities = presetApp?.List
                .OfType(LicenseEntity.TypeNameId)
                .Select(e => new LicenseEntity(e))
                .ToList()
                ?? [];
            l.A($"licEnt:{licEntities.Count}");

            // Check all licenses and show extra message
            var enterpriseLicenses = licEntities
                .Select(lic =>
                {
                    var entFp = lic.AsEnterprise();
                    l.A(entFp.ValidityMessage);
                    return entFp;
                })
                .ToList();
            l.A($"entLic:{enterpriseLicenses.Count}");

            licenseLoader
                .Init(enterpriseLicenses)
                .LoadLicenses();
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
        var stored = LoadFeaturesStored();
        var status = SetFeaturesStored(stored);
        return l.ReturnAndLog(status);
    }


    private bool SetFeaturesStored(FeatureStatesPersisted stored = null) 
        => featuresSvc.UpdateFeatureList(stored ?? new FeatureStatesPersisted(), sysFeaturesService.States);


    /// <summary>
    /// Load features stored from 'features.json'.
    /// When old format is detected, it is converted to new format.
    /// </summary>
    /// <returns></returns>
    internal FeatureStatesPersisted LoadFeaturesStored()
    {
        var l = Log.Fn<FeatureStatesPersisted>();
        try
        {
            var (filePath, fileContent) = featurePersistenceService.LoadFeaturesFile();
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileContent)) 
                return l.ReturnNull("ok, but 'features.json' is missing");

            // handle old 'features.json' format
            var oldState = featurePersistenceService.ConvertOldFeaturesFile(filePath, fileContent);
            if (oldState != null) 
                return l.ReturnAndLog(oldState, "converted to new features.json");

            // return features stored
            var newState = JsonSerializer.Deserialize<FeatureStatesPersisted>(fileContent, JsonOptions.UnsafeJsonWithoutEncodingHtml);

            return l.ReturnAndLog(newState, "ok, features loaded");
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
    public bool UpdateFeatures(List<FeatureManagementChange> changes)
    {
        var l = Log.Fn<bool>($"c:{changes?.Count ?? -1}");
        var saved = featurePersistenceService.SaveFeaturesUpdate(changes);
        var newState = FeatureListStoredBuilder(changes);
        SetFeaturesStored(newState);
        return l.ReturnAndLog(saved, "ok, updated");
    }

    private FeatureStatesPersisted FeatureListStoredBuilder(List<FeatureManagementChange> changes)
    {
        var updatedIds = changes
            .Select(f => f.FeatureGuid)
            .ToList();

        var storedFeaturesButNotUpdated = featuresSvc.All
            .Where(f => f.EnabledInConfiguration.HasValue && !updatedIds.Contains(f.Aspect.Guid))
            .Select(FeaturePersistenceService.FeatureConfigBuilder)
            .ToList();
            
        var updatedFeatures = changes
            .Where(f => f.Enabled.HasValue)
            .Select(FeaturePersistenceService.FeatureConfigBuilder)
            .ToList();

        return new()
        {
            Features = storedFeaturesButNotUpdated
                .Union(updatedFeatures)
                .ToList(),
            Fingerprint = fingerprint.GetFingerprint()
        };
    }
}