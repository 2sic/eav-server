using System;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Repository.Efc.Implementations
{
	/// <summary>
	/// Global Eav Configuration
	/// </summary>
	public class Configuration : ISystemConfiguration
	{
	    private static string _conStr;

	    /// <summary>
	    /// Set the string itself as needed
	    /// </summary>
	    /// <param name="conString"></param>
	    public static void SetConnectionString(string conString) => _conStr = conString;

        #region Internal delivery for depedency injection...
	    /// <inheritdoc />
	    public string DbConnectionString {
	        get
	        {
	            if (!string.IsNullOrEmpty(_conStr))
	                return _conStr;
	            throw new Exception("Couldn't load Connection String as SetConnectionString must have been forgotten");

	        }
	    }

	    /// <inheritdoc />
	    public string FeaturesHelpLink => _featuresHelpLink;

	    /// <inheritdoc />
	    public string FeatureInfoLinkRoot => _featureInfoLinkRoot;


	    private static string _featuresHelpLink;
	    private static string _featureInfoLinkRoot;

	    public static void SetFeaturesHelpLink(string helpLink, string infoLink)
	    {
	        _featuresHelpLink = helpLink;
	        _featureInfoLinkRoot = infoLink;
	    }

	    #endregion

	}
}
