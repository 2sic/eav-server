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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Encryption;

namespace ToSic.Eav.Configuration.Licenses
{
    /// <summary>
    /// Will check the loaded licenses and prepare validity information for use during system runtime
    /// </summary>
    internal sealed class LicenseLoader: LoaderBase
    {
        /// <summary>
        /// Constructor - not meant for DI
        /// </summary>
        internal LicenseLoader(LogHistory logHistory, LicenseCatalog licenseCatalog, ILog parentLog)
            : base(logHistory, parentLog, LogNames.Eav + "LicLdr", "Load Licenses")
        {
            _licenseCatalog = licenseCatalog;
        }
        private readonly LicenseCatalog _licenseCatalog;

        /// <summary>
        /// Pre-Load enabled / disabled global features
        /// </summary>
        [PrivateApi]
        internal void LoadLicenses(string fingerprint, string globalFolder)
        {
            var wrapLog = Log.Fn(startTimer: true);
            try
            {
                var licensesStored = LicensesStoredInConfigFolder(globalFolder);
                Log.A($"Found {licensesStored.Count} licenseStored in files");
                var licenses = licensesStored.SelectMany(ls => LicensesStateBuilder(ls, fingerprint)).ToList();
                var autoEnabled = AutoEnabledLicenses();
                LicenseService.Update(autoEnabled.Union(licenses).ToList());
                Log.A($"Found {licenses.Count} licenses");
                wrapLog.Done("ok");
            }
            catch (Exception ex)
            {
                // Just log and ignore
                Log.Ex(ex);
                wrapLog.Done("error");
            }
        }

        public List<LicenseStored> LicensesStoredInConfigFolder(string globalFolder)
        {
            var wrapLog = Log.Call<List<LicenseStored>>();
            var configurationsPath = Path.Combine(globalFolder, Constants.FolderDataCustom, FsDataConstants.ConfigFolder);
            
            // ensure that path to store files already exits
            Directory.CreateDirectory(configurationsPath);

            var licensesStored = Directory.EnumerateFiles(configurationsPath, "*.license.json")
                .Select(File.ReadAllText)
                .Select(JsonConvert.DeserializeObject<LicenseStored>)
                .Where(licenses => licenses != null).ToList();

            Log.A($"licensesStored: {licensesStored.Count}");
            return wrapLog("ok", licensesStored);
        }

        private List<LicenseState> LicensesStateBuilder(LicenseStored licenseStored, string fingerprint)
        {
            var wrapLog = Log.Call<List<LicenseState>>();

            if (licenseStored == null) return wrapLog("null", new List<LicenseState>());

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
                Log.Ex(ex);
            }
            Log.A($"Signature: {validSig}");

            // Check fingerprints
            var fps = licenseStored.FingerprintsArray;
            var validFp = fps.Any(fingerprint.Equals);
            Log.A($"Fingerprint: {validFp}");

            var validVersion = licenseStored.Versions?
                .Split(',')
                .Select(v => v.Trim())
                .Any(v => int.TryParse(v, out var licVersion) && SystemInformation.Version.Major == licVersion)
                ?? false;

            Log.A($"Version: {validVersion}");

            var validDate = DateTime.Now.CompareTo(licenseStored.Expires) <= 0;
            Log.A($"Expired: {validDate}");

            var licenses = licenseStored.LicensesArray;
            Log.A($"Licenses: {licenses.Length}");

            var licenseStates = licenses.Select(l => new LicenseState
                {
                    Title = licenseStored.Title,
                    License = _licenseCatalog.TryGet(l),
                    EntityGuid = licenseStored.GuidSalt,
                    LicenseKey = licenseStored.Key,
                    Expiration = licenseStored.Expires,
                    ValidExpired = validDate,
                    ValidFingerprint = validFp,
                    ValidSignature = validSig,
                    ValidVersion = validVersion,
                    Owner = licenseStored.Owner,
                })
                .ToList();

            return wrapLog(licenseStates.Count.ToString(), licenseStates);
        }
        

        private List<LicenseState> AutoEnabledLicenses()
        {
            var licenseStates = _licenseCatalog.List.Where(l => l.AutoEnable).Select(l => new LicenseState
                {
                    Title = l.Name,
                    License = l,
                    EntityGuid = Guid.Empty,
                    LicenseKey = "always enabled",
                    Expiration = BuiltInLicenses.UnlimitedExpiry,
                    ValidExpired = true,
                    ValidFingerprint = true,
                    ValidSignature = true,
                    ValidVersion = true,
                })
                .ToList();
            return licenseStates;
        }
    }
}
