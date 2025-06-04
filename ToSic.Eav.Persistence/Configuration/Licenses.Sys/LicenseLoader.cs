/*
 * Copyright 2022 by 2sic internet solutions in Switzerland - www.2sic.com
 *
 * This file and the code IS COPYRIGHTED.
 * 1. You may not change it.
 * 2. You may not copy the code to reuse in another way.
 *
 * Copying this or creating a similar service, 
 * especially when used to circumvent licensing features in EAV and 2sxc
 * is a copyright infringement.
 *
 * Please remember that 2sic has sponsored more than 10 years of work,
 * and paid more than 1 Million USD in wages for its development.
 * So asking for support to finance advanced features is not asking for much. 
 *
 */

using System.Text;
using System.Text.Json;
using ToSic.Eav.Security.Encryption;
using ToSic.Eav.Sys.Configuration;
using ToSic.Sys.Capabilities;
using ToSic.Sys.Capabilities.FeatureSet;
using ToSic.Sys.Capabilities.Fingerprints;
using ToSic.Sys.Capabilities.Licenses;
using ToSic.Sys.Configuration;
using ToSic.Sys.Startup;

namespace ToSic.Eav.Configuration.Sys.Loaders;

/// <summary>
/// Will check the loaded licenses and prepare validity information for use during system runtime
/// </summary>
/// <remarks>Must be SEALED, to prevent inheritance and prevent injection of alternate loader</remarks>
public sealed class LicenseLoader(
    ILogStore logStore,
    LicenseCatalog licenseCatalog,
    SystemFingerprint fingerprint,
    LazySvc<IGlobalConfiguration> globalConfiguration)
    : LoaderBase(logStore, $"{EavLogs.Eav}LicLdr", connect: [licenseCatalog, fingerprint, globalConfiguration])
{
    internal LicenseLoader Init(List<EnterpriseFingerprint> entFingerprints)
    {
        fingerprint.LoadEnterpriseFingerprints(entFingerprints);
        return this;
    }

    /// <summary>
    /// Pre-Load enabled / disabled global features
    /// </summary>
    [PrivateApi]
    internal void LoadLicenses()
    {
        var l = this.Log.Fn(message: "start", timer: true);
        var fingerprint1 = fingerprint.GetFingerprint();
        l.A($"fingerprint:{fingerprint1?.Length}");

        var validEntFps = fingerprint.EnterpriseFingerprints
            .Where(e => e.Valid)
            .ToList();
        l.A($"validEntFps:{validEntFps.Count}");

        try
        {
            var licensesStored = LoadLicensesInConfigFolder();
            l.A($"Found {licensesStored.Count} licenseStored in files");
            var licenses = licensesStored
                .SelectMany(ls => LicensesStateBuilder(ls, fingerprint1, validEntFps))
                .ToList();
            var autoEnabled = AutoEnabledLicenses();
            LicenseService.Update(autoEnabled.Union(licenses).ToList());
            l.Done($"Found {licenses.Count} licenses");
        }
        catch (Exception ex)
        {
            // Just log and ignore
            l.Ex(ex);
            l.Done("error");
        }
    }

    /// <summary>
    /// Load the license JSON files
    /// </summary>
    /// <returns></returns>
    private List<LicensesPersisted> LoadLicensesInConfigFolder()
    {
        var l = Log.Fn<List<LicensesPersisted>>();
        // ensure that path to store files already exits
        var configFolder = globalConfiguration.Value.ConfigFolder();
        Directory.CreateDirectory(configFolder);
        l.A($"{nameof(configFolder)}: {configFolder}");

        var licensesStored = Directory.EnumerateFiles(configFolder, "*.license.json")
            .Select(File.ReadAllText)
            .Select(j => JsonSerializer.Deserialize<LicensesPersisted>(j)) // should NOT use common SxcJsonSerializerOptions
            .Where(licenses => licenses != null)
            .ToList();

        return l.Return(licensesStored, $"licensesStored: {licensesStored.Count}");
    }

    private List<FeatureSetState> LicensesStateBuilder(LicensesPersisted licensesPersisted, string fingerprint, List<EnterpriseFingerprint> validEntFps)
    {
        var l = Log.Fn<List<FeatureSetState>>();
        if (licensesPersisted == null)
            return l.Return([], "null");

        // Check signature valid
        var resultForSignature = licensesPersisted.GenerateIdentity();
        var validSig = false;
        try
        {
            var data = new UnicodeEncoding().GetBytes(resultForSignature);
            validSig = new Sha256().VerifyBase64(FeatureConstants.FeaturesValidationSignature2Sxc930,
                licensesPersisted.Signature, data);
        }
        catch (Exception ex)
        {
            // Just log, and ignore
            l.Ex(ex);
        }
        l.A($"Signature: {validSig}");

        // Check fingerprints
        var fps = licensesPersisted.FingerprintsArray;
        var validFp = fps.Any(fingerprint.Equals) || validEntFps.Any(ld => fps.Any(ld.Fingerprint.Equals));
        l.A($"Fingerprint: {validFp}");

        var validVersion = licensesPersisted.Versions?
                               .CsvToArrayWithoutEmpty()
                               .Any(v => int.TryParse(v, out var licVersion) && EavSystemInfo.Version.Major == licVersion)
                           ?? false;

        l.A($"Version: {validVersion}");

        var validDate = DateTime.Now.CompareTo(licensesPersisted.Expires) <= 0;
        l.A($"Expired: {validDate}");

        var licenses = licensesPersisted?.Licenses ?? [];
        l.A($"Licenses: {licenses.Count}");


        var licenseStates = licenses
            .Where(storedDetails => !string.IsNullOrEmpty(storedDetails.Id))
            .Select((storedDetails, index) =>
            {
                var licDef = licenseCatalog.TryGet(storedDetails.Id);

                // If no real license found with this ID, it's probably a single-feature activation
                // For this we must add a virtual license for this feature only
                if (licDef == null)
                {
                    licDef = new()
                    {
                        NameId = LicenseConstants.LicenseCustom,
                        Priority = LicenseConstants.FeatureLicensesBaseId + index,
                        Name = storedDetails.Comments ?? "Feature (unknown)",
                        Guid = Guid.TryParse(storedDetails.Id, out var guidId) ? guidId : Guid.Empty,
                        Description = $"Feature: {storedDetails.Comments} ({storedDetails.Id})",
                        FeatureLicense = true
                    };
                    l.A(
                        $"Virtual/Feature license detected. Add virtual license to enable activation for {licDef.NameId} - {licDef.Guid}");
                    licenseCatalog.Register(licDef);
                }

                return new FeatureSetState(licDef)
                {
                    Title = licensesPersisted.Title,
                    EntityGuid = licensesPersisted.GuidSalt,
                    LicenseKey = licensesPersisted.Key ?? LicenseKeyDescription,
                    Expiration = storedDetails.Expires ?? licensesPersisted.Expires,
                    ExpirationIsValid = DateTime.Now.CompareTo(storedDetails.Expires ?? licensesPersisted.Expires) <= 0,
                    FingerprintIsValid = validFp,
                    SignatureIsValid = validSig,
                    VersionIsValid = validVersion,
                    Owner = licensesPersisted.Owner,
                };
            })
            .ToList();

        return l.Return(licenseStates, $"count: {licenseStates.Count}");
    }

    // license key description on this system
    private const string LicenseKeyDescription = "system license";

    /// <summary>
    /// Get list of licenses which are always auto-enabled
    /// </summary>
    /// <returns></returns>
    private List<FeatureSetState> AutoEnabledLicenses()
    {
        var licenseStates = licenseCatalog.List
            .Where(ls => ls.AutoEnable)
            .Select(ls => new FeatureSetState(ls)
            {
                Title = ls.Name,
                EntityGuid = Guid.Empty,
                LicenseKey = "always enabled",
                Expiration = LicenseConstants.UnlimitedExpiry,
                ExpirationIsValid = true,
                FingerprintIsValid = true,
                SignatureIsValid = true,
                VersionIsValid = true,
            })
            .ToList();
        return licenseStates;
    }
}