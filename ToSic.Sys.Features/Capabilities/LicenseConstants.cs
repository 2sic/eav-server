﻿namespace ToSic.Sys.Capabilities;

public class LicenseConstants
{
    public const int TestLicensesBaseId = 9000;
    public const int FeatureLicensesBaseId = 9900;
    public const string LicensePrefix = "License-";
    public const string LicenseCustom = LicensePrefix + "Custom";
    public const string FeatureSetSystem = "System";   // Feature Set "System"
    public const string FeatureSetExtension = "Extension";

    public static readonly DateTime UnlimitedExpiry = DateTime.MaxValue;

}