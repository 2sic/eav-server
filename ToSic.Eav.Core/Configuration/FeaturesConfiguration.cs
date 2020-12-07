namespace ToSic.Eav.Configuration
{
	/// <summary>
	/// Global Eav Configuration
	/// </summary>
	public class FeaturesConfiguration : IFeaturesConfiguration
	{
        /// <inheritdoc />
	    public string FeaturesHelpLink
        {
            get => _featuresHelpLink;
            set => _featuresHelpLink = value;
        }

        /// <inheritdoc />
	    public string FeatureInfoLinkRoot
        {
            get => _featureInfoLinkRoot;
            set => _featureInfoLinkRoot = value;
        }


        private static string _featuresHelpLink = "https://2sxc.org/help?tag=features";

        private static string _featureInfoLinkRoot = "https://2sxc.org/r/f/";

    }
}
