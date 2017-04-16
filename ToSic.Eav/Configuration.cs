using System;
using System.Configuration;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav
{
	/// <summary>
	/// Global Eav Configuration
	/// </summary>
	public class Configuration : ISystemConfiguration
	{
		private const string DefaultConnectionStringName = "EavContext";
		private static string _connectionStringName;

		/// <summary>
		/// Set ConnectionStringName for current Web Application. When set it's keeped while the IIS Application is running.
		/// </summary>
		/// <param name="connectionStringName">Name of the ConnectionString in web.config</param>
		public static void SetConnectionString(string connectionStringName)
		{
			_connectionStringName = connectionStringName;
		}

		/// <summary>
		/// Builds a Connection String that's usable by EntityFramework.
		/// SetConnectionString must be called before (if not, DefaultConnectionStringName is used).
		/// ConnectionString must be a simple SQL Connection string (not a Entity Framework one).
		/// </summary>
		/// <returns>ConnectionString or fallback from config for dev purpose</returns>

	    internal static string ConnectionString(string key)
	    {
            try
            {
                return ConfigurationManager.ConnectionStrings[key].ConnectionString;
            }
            catch (NullReferenceException)
            {
                throw new Exception("Couldn't load Connection String with name \"" + key + "\"");
            }
        }

        #region Internal depedency injection...

	    /// <summary>
	    /// Db Connection String used in the Eav-Connector
	    /// </summary>
	    public string DbConnectionString => ConnectionString(string.IsNullOrEmpty(_connectionStringName)
                ? DefaultConnectionStringName
                : _connectionStringName);

	    #endregion

    }
}
