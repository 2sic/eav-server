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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Encryption;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Configuration.Licenses
{
    /// <summary>
    /// Will check the loaded licenses and prepare validity information for use during system runtime
    /// </summary>
    /// <remarks>Must be SEALED, to prevent inheritance and prevent injection of alternate loader</remarks>
    public sealed class LicenseLoader : LoaderBase
    {
        /// <summary>
        /// Constructor - for DI
        /// </summary>
        public LicenseLoader(
            ILogStore logStore,
            LicenseCatalog licenseCatalog,
            SystemFingerprint fingerprint,
            LazySvc<IGlobalConfiguration> globalConfiguration
        ) : base(logStore, $"{EavLogs.Eav}LicLdr")
        {
            Log.A("Load Licenses");
            ConnectServices(
                _licenseCatalog = licenseCatalog,
                _fingerprint = fingerprint,
                _globalConfiguration = globalConfiguration
            );
        }

        internal LicenseLoader Init(List<EnterpriseFingerprint> entFingerprints)
        {
            _fingerprint.LoadEnterpriseFingerprintsWIP(entFingerprints);
            return this;
        }

        private readonly LicenseCatalog _licenseCatalog;
        private readonly SystemFingerprint _fingerprint;
        private readonly LazySvc<IGlobalConfiguration> _globalConfiguration;

        /// <summary>
        /// Pre-Load enabled / disabled global features
        /// </summary>
        [PrivateApi]
        internal void LoadLicenses() => Log.Do(timer: true, action: l=>
        {
            var fingerprint = _fingerprint.GetFingerprint();
            l.A($"fingerprint:{fingerprint?.Length}");

            var validEntFps = _fingerprint.EnterpriseFingerprintsWIP
                .Where(e => e.Valid)
                .ToList();
            l.A($"validEntFps:{validEntFps?.Count}");

            try
            {
                var licensesStored = LoadLicensesInConfigFolder();
                l.A($"Found {licensesStored.Count} licenseStored in files");
                var licenses = licensesStored.SelectMany(ls => LicensesStateBuilder(ls, fingerprint, validEntFps)).ToList();
                var autoEnabled = AutoEnabledLicenses();
                LicenseService.Update(autoEnabled.Union(licenses).ToList());
                l.A($"Found {licenses.Count} licenses");
                return "ok";
            }
            catch (Exception ex)
            {
                // Just log and ignore
                l.Ex(ex);
                return "error";
            }
        });

        /// <summary>
        /// Load the license JSON files
        /// </summary>
        /// <returns></returns>
        private List<LicenseStored> LoadLicensesInConfigFolder()
        {
            var l = Log.Fn<List<LicenseStored>>();
            // ensure that path to store files already exits
            var configFolder = _globalConfiguration.Value.ConfigFolder;
            Directory.CreateDirectory(configFolder);
            l.A($"{nameof(configFolder)}: {configFolder}");

            var licensesStored = Directory.EnumerateFiles(configFolder, "*.license.json")
                .Select(File.ReadAllText)
                .Select(j => JsonSerializer.Deserialize<LicenseStored>(j)) // should NOT use common SxcJsonSerializerOptions
                .Where(licenses => licenses != null).ToList();

            return l.Return(licensesStored, $"licensesStored: {licensesStored.Count}");
        }

        private List<LicenseState> LicensesStateBuilder(LicenseStored licenseStored, string fingerprint, List<EnterpriseFingerprint> validEntFps)
        {
            var l = Log.Fn<List<LicenseState>>();
            if (licenseStored == null) return l.Return(new List<LicenseState>(), "null");

            // Check signature valid
            var resultForSignature = licenseStored.GenerateIdentity();
            var validSig = false;
            try
            {
                var data = new UnicodeEncoding().GetBytes(resultForSignature);
                validSig = new Sha256().VerifyBase64(FeatureConstants.FeaturesValidationSignature2Sxc930,
                    licenseStored.Signature, data);
            }
            catch (Exception ex)
            {
                // Just log, and ignore
                l.Ex(ex);
            }
            l.A($"Signature: {validSig}");

            // Check fingerprints
            var fps = licenseStored.FingerprintsArray;
            var validFp = fps.Any(fingerprint.Equals) || validEntFps.Any(ld => fps.Any(ld.Fingerprint.Equals));
            l.A($"Fingerprint: {validFp}");

            var validVersion = licenseStored.Versions?
                .Split(',')
                .Select(v => v.Trim())
                .Any(v => int.TryParse(v, out var licVersion) && SystemInformation.Version.Major == licVersion)
                ?? false;

            l.A($"Version: {validVersion}");

            var validDate = DateTime.Now.CompareTo(licenseStored.Expires) <= 0;
            l.A($"Expired: {validDate}");

            var licenses = licenseStored?.Licenses ?? new List<LicenseStoredDetails>();
            l.A($"Licenses: {licenses.Count}");


            var licenseStates = licenses
                .Where(storedDetails => !string.IsNullOrEmpty(storedDetails.Id))
                .Select((storedDetails, index) =>
                {
                    var licDef = _licenseCatalog.TryGet(storedDetails.Id);

                    // If no real license found with this ID, it's probably a single-feature activation
                    // For this we must add a virtual license for this feature only
                    if (licDef == null)
                    {
                        licDef = new LicenseDefinition(BuiltInLicenses.LicenseCustom, BuiltInLicenses.FeatureLicensesBaseId + index, storedDetails.Comments ?? "Feature (unknown)",
                            Guid.TryParse(storedDetails.Id, out var guidId) ? guidId : Guid.Empty,
                            $"Feature: {storedDetails.Comments} ({storedDetails.Id})", featureLicense: true);
                        l.A($"Virtual/Feature license detected. Add virtual license to enable activation for {licDef.NameId} - {licDef.Guid}");
                        _licenseCatalog.Register(licDef);
                    }

                    return new LicenseState
                    {
                        Title = licenseStored.Title,
                        License = licDef,
                        EntityGuid = licenseStored.GuidSalt,
                        LicenseKey = licenseStored.Key ?? LicenseKeyDescription,
                        Expiration = storedDetails.Expires ?? licenseStored.Expires,
                        ExpirationIsValid = DateTime.Now.CompareTo(storedDetails.Expires ?? licenseStored.Expires) <= 0,
                        FingerprintIsValid = validFp,
                        SignatureIsValid = validSig,
                        VersionIsValid = validVersion,
                        Owner = licenseStored.Owner,
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
        private List<LicenseState> AutoEnabledLicenses()
        {
            var licenseStates = _licenseCatalog.List.Where(l => l.AutoEnable).Select(l => new LicenseState
                {
                    Title = l.Name,
                    License = l,
                    EntityGuid = Guid.Empty,
                    LicenseKey = "always enabled",
                    Expiration = BuiltInLicenses.UnlimitedExpiry,
                    ExpirationIsValid = true,
                    FingerprintIsValid = true,
                    SignatureIsValid = true,
                    VersionIsValid = true,
                })
                .ToList();
            return licenseStates;
        }
    }
}
