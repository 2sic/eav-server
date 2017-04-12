using System;
using System.Data.EntityClient;
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
		/// <returns>ConnectionString for the EntityFramework</returns>
		internal static string GetConnectionString()
		{
			if (!string.IsNullOrEmpty(_connectionStringName))
			{
				//string ConnectionStringName = HttpContext.Current.Application[AppSettingPrefix + "." + ConnectionStringSetting].ToString();
				var connectionStringName = _connectionStringName;

				var builder = new EntityConnectionStringBuilder();
				try
				{
					builder.ProviderConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
				}
				catch (NullReferenceException)
				{
					throw new Exception("Couldn't load Connection String with name \"" + connectionStringName + "\"");
				}
				if (!builder.ProviderConnectionString.Contains("MultipleActiveResultSets"))
					builder.ProviderConnectionString += ";MultipleActiveResultSets=True";

			    builder.Metadata = "res://*/Persistence.EavContext.csdl|res://*/Persistence.EavContext.ssdl|res://*/Persistence.EavContext.msl";
				builder.Provider = "System.Data.SqlClient";

				return builder.ToString();
			}
			try
			{
				return ConfigurationManager.ConnectionStrings[DefaultConnectionStringName].ConnectionString;
			}
			catch (NullReferenceException)
			{
				throw new Exception("Couldn't load Connection String with name \"" + DefaultConnectionStringName + "\"");
			}
        }

        #region Internal depedency injection...

	    /// <summary>
	    /// Db Connection String used in the Eav-Connector
	    /// </summary>
	    public string DbConnectionString => GetConnectionString();

	    #endregion

    }
}
