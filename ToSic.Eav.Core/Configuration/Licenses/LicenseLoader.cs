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
        internal LicenseLoader(LogHistory logHistory, ILog parentLog)
            : base(logHistory, parentLog, LogNames.Eav + "LicLdr", "Load Licenses")
        {
        }

        /// <summary>
        /// Pre-Load enabled / disabled global features
        /// </summary>
        [PrivateApi]
        internal void LoadLicenses(string fingerprint, string globalFolder)
        {
            var wrapLog = Log.Call();
            try
            {
                var licensesStored = LicensesStoredInConfigFolder(globalFolder);
                Log.Add($"Found {licensesStored.Count} licenseStored in files");
                var licenses = licensesStored.SelectMany(ls => LicensesStateBuilder(ls, fingerprint)).ToList();
                var autoEnabled = AutoEnabledLicenses();
                LicenseService.Update(autoEnabled.Union(licenses).ToList());
                Log.Add($"Found {licenses.Count} licenses");
                wrapLog("ok");
            }
            catch (Exception ex)
            {
                // Just log and ignore
                Log.Exception(ex);
                wrapLog("error");
            }
        }

        public List<LicenseStored> LicensesStoredInConfigFolder(string globalFolder)
        {
            var wrapLog = Log.Call<List<LicenseStored>>();
            var configurationsPath = Path.Combine(globalFolder, Constants.FolderDataCustom, FsDataConstants.ConfigFolder);

            var licensesStored = Directory.EnumerateFiles(configurationsPath, "*.license.json")
                .Select(File.ReadAllText)
                .Select(JsonConvert.DeserializeObject<LicenseStored>)
                .Where(licenses => licenses != null).ToList();

            Log.Add($"licensesStored: {licensesStored.Count}");
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
                Log.Exception(ex);
            }
            Log.Add($"Signature: {validSig}");

            // Check fingerprints
            var fps = licenseStored.FingerprintsArray;
            var validFp = fps.Any(fingerprint.Equals);
            Log.Add($"Fingerprint: {validFp}");

            var validVersion = licenseStored.Versions?
                .Split(',')
                .Select(v => v.Trim())
                .Any(v => int.TryParse(v, out var licVersion) && SystemInformation.Version.Major == licVersion)
                ?? false;

            Log.Add($"Version: {validVersion}");

            var validDate = DateTime.Now.CompareTo(licenseStored.Expires) <= 0;
            Log.Add($"Expired: {validDate}");

            var licenses = licenseStored.LicensesArray;
            Log.Add($"Licenses: {licenses.Length}");

            var licenseStates = licenses.Select(l => new LicenseState
                {
                    Title = licenseStored.Title,
                    License = LicenseCatalog.Find(l),
                    EntityGuid = licenseStored.GuidSalt,
                    LicenseKey = licenseStored.Key,
                    Expiration = licenseStored.Expires,
                    ValidExpired = validDate,
                    ValidFingerprint = validFp,
                    ValidSignature = validSig,
                    ValidVersion = validVersion,
                })
                .ToList();

            return wrapLog(licenseStates.Count.ToString(), licenseStates);
        }

        //private List<LicenseState> LicensesInOneEntity(IEntity entity)
        //{
        //    var wrapLog = Log.Call<List<LicenseState>>();

        //    if (entity == null) return wrapLog("null", new List<LicenseState>());

        //    var infoRaw = new LicenseInfoRawOld(entity);

        //    // Check signature valid
        //    var resultForSignature = infoRaw.GetStandardizedControlString();
        //    var validSig = false;
        //    try
        //    {
        //        var data = new UnicodeEncoding().GetBytes(resultForSignature);
        //        validSig = new Sha256().VerifyBase64(FeatureConstants.FeaturesValidationSignature2Sxc930,
        //            infoRaw.Signature, data);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Just log, and ignore
        //        Log.Exception(ex);
        //    }
        //    Log.Add($"Signature: {validSig}");

        //    // Check fingerprints
        //    //var myFingerprint = Fingerprint;
        //    var fps = infoRaw.Fingerprints.SplitNewLine().TrimmedAndWithoutEmpty();
        //    var validFp = fps.Any(fp => _fingerprint.Equals(fp));
        //    Log.Add($"Fingerprint: {validFp}");

        //    var validVersion = int.TryParse(infoRaw.Version, out var licVersion) &&
        //                       SystemInformation.Version.Major == licVersion;
        //    Log.Add($"Version: {validVersion}");

        //    var dateOk = DateTime.TryParse(infoRaw.Expires, out var expires);
        //    if (!dateOk)
        //        expires = DateTime.MinValue;

        //    var validDate = dateOk && DateTime.Now.CompareTo(expires) <= 0;
        //    Log.Add($"Expired: {validDate}");

        //    var licenses = infoRaw.Licenses.SplitNewLine().TrimmedAndWithoutEmpty() ?? Array.Empty<string>();
        //    Log.Add($"Licenses: {licenses.Length}");

        //    var licenseStates = licenses.Select(l => new LicenseState
        //    {
        //        Title = infoRaw.Name,
        //        License = LicenseCatalog.Find(l),
        //        EntityGuid = infoRaw.Guid,
        //        LicenseKey = infoRaw.Key,
        //        Expiration = expires,
        //        ValidExpired = validDate,
        //        ValidFingerprint = validFp,
        //        ValidSignature = validSig,
        //        ValidVersion = validVersion,
        //    })
        //        .ToList();

        //    return wrapLog(licenseStates.Count.ToString(), licenseStates);
        //}

        private List<LicenseState> AutoEnabledLicenses()
        {
            var licenseStates = LicenseCatalog.Licenses.Where(l => l.AutoEnable).Select(l => new LicenseState
                {
                    Title = l.Name,
                    License = l,
                    EntityGuid = Guid.Empty,
                    LicenseKey = "always enabled",
                    Expiration = LicenseCatalog.UnlimitedExpiry,
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
