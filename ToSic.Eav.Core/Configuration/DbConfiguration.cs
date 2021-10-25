using System;

namespace ToSic.Eav.Configuration
{
	/// <summary>
	/// Global Eav Configuration
	/// </summary>
	public class DbConfiguration : IDbConfiguration
	{
	    private static string _conStr;

        #region Internal delivery for depedency injection...
	    /// <inheritdoc />
	    public string ConnectionString
        {
            get => _conStr 
                   ?? throw new Exception("Couldn't load Connection String as SetConnectionString must have been forgotten");
            set => _conStr = value;
        }

     //   /// <inheritdoc />
	    //public string FeaturesHelpLink => _featuresHelpLink;

	    ///// <inheritdoc />
	    //public string FeatureInfoLinkRoot => _featureInfoLinkRoot;


	    private static string _featuresHelpLink;
	    private static string _featureInfoLinkRoot;

	    /// <summary>
	    /// TODO
	    /// </summary>
	    /// <param name="helpLink"></param>
	    /// <param name="infoLink"></param>
	    public static void SetFeaturesHelpLink(string helpLink, string infoLink)
	    {
	        _featuresHelpLink = helpLink;
	        _featureInfoLinkRoot = infoLink;
	    }

	    #endregion

	}
}
