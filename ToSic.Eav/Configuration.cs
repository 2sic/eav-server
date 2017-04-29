using System;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav
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
	    /// <summary>
	    /// Db Connection String used in the Eav-Connector
	    /// </summary>
	    public string DbConnectionString {
	        get
	        {
	            if (!string.IsNullOrEmpty(_conStr))
	                return _conStr;
	            throw new Exception("Couldn't load Connection String as SetConnectionString must have been forgotten");

	        }
	    }

        #endregion

    }
}
