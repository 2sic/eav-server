using System;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav
{
	/// <summary>
	/// Global Eav Configuration
	/// </summary>
	public class Configuration : ISystemConfiguration
	{
		//private const string DefaultConnectionStringName = "EavContext";
		//private static string _conStrKey;
	    private static string _conStr;

		///// <summary>
		///// Set ConnectionStringName for current Web Application. When set it's keeped while the IIS Application is running.
		///// </summary>
		///// <param name="connectionStringName">Name of the ConnectionString in web.config</param>
		//public static void SetConnectionStringKey(string connectionStringName) => _conStrKey = connectionStringName;

	    /// <summary>
	    /// Set the string itself as needed
	    /// </summary>
	    /// <param name="conString"></param>
	    public static void SetConnectionString(string conString) => _conStr = conString;

		/// <summary>
		/// Builds a Connection String that's usable by EntityFramework.
		/// SetConnectionString must be called before (if not, DefaultConnectionStringName is used).
		/// ConnectionString must be a simple SQL Connection string (not a Entity Framework one).
		/// </summary>
		/// <returns>ConnectionString or fallback from config for dev purpose</returns>

	    internal static string ConnectionString()
	    {
            //try
            //{
                if(!string.IsNullOrEmpty(_conStr))
                    return _conStr;
                throw new Exception("Couldn't load Connection String as SetConnectionString must have been forgotten");
                //ConfigurationManager.ConnectionStrings[(string.IsNullOrEmpty(_conStrKey)
                //        ? DefaultConnectionStringName
                //        : _conStrKey)].ConnectionString;
                // return ConfigurationManager.ConnectionStrings[key].ConnectionString;
            //}
            //catch (NullReferenceException)
            //{
            //    throw new Exception("Couldn't load Connection String with with value \"" + _conStr + "\" or name \"" + _conStrKey + "\"");
            //}
        }

        #region Internal depedency injection...

	    /// <summary>
	    /// Db Connection String used in the Eav-Connector
	    /// </summary>
	    public string DbConnectionString => ConnectionString();

	    //string.IsNullOrEmpty(_conStr) ? _conStr : ConnectionString(string.IsNullOrEmpty(_conStrKey)
	    //    ? DefaultConnectionStringName
	    //    : _conStrKey);

	    #endregion

	}
}
