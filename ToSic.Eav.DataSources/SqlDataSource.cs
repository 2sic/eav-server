﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Provide Entities from a SQL Server
	/// </summary>
	public class SqlDataSource : ExternalDataDataSource // BaseDataSource
	{
        // Note: of the standard SQL-terms, I will only allow exec|execute|select
        // Everything else shouldn't be allowed
        public static Regex ForbiddenTermsInSelect = new Regex(@"(;|\s|^)+(insert|update|delete|create|alter|drop|rename|truncate|backup|restore)\s", RegexOptions.IgnoreCase);
		#region Configuration-properties
		protected const string TitleFieldKey = "TitleField";
		protected const string EntityIdFieldKey = "EntityIdField";
		protected const string ContentTypeKey = "ContentType";
		protected const string SelectCommandKey = "SelectCommand";
		protected const string ConnectionStringKey = "ConnectionString";
		protected const string ConnectionStringNameKey = "ConnectionStringName";
		protected const string ConnectionStringDefault = "[Settings:ConnectionString]";

		/// <summary>
		/// Default Name of the EntityId Column
		/// </summary>
		public static readonly string EntityIdDefaultColumnName = "EntityId";

		/// <summary>
		/// Default Name of the EntityTitle Column
		/// </summary>
		public static readonly string EntityTitleDefaultColumnName = "EntityTitle";

		/// <summary>
		/// Gets or sets the name of the ConnectionString in the Application.Config to use
		/// </summary>
		public string ConnectionStringName
		{
			get { return Configuration[ConnectionStringNameKey]; }
			set { Configuration[ConnectionStringNameKey] = value; }
		}

		/// <summary>
		/// Gets or sets the ConnectionString
		/// </summary>
		public string ConnectionString
		{
			get { return Configuration[ConnectionStringKey]; }
			set { Configuration[ConnectionStringKey] = value; }
		}

		/// <summary>
		/// Gets or sets the SQL Command
		/// </summary>
		public string SelectCommand
		{
			get { return Configuration[SelectCommandKey]; }
			set { Configuration[SelectCommandKey] = value; }
		}

		/// <summary>
		/// Gets or sets the Name of the ContentType
		/// </summary>
		public string ContentType
		{
			get { return Configuration[ContentTypeKey]; }
			set { Configuration[ContentTypeKey] = value; }
		}

		/// <summary>
		/// Gets or sets the Name of the Title Attribute of the Source DataTable
		/// </summary>
		public string TitleField
		{
			get { return Configuration[TitleFieldKey]; }
			set { Configuration[TitleFieldKey] = value; }
		}

		/// <summary>
		/// Gets or sets the Name of the Column used as EntityId
		/// </summary>
		public string EntityIdField
		{
			get { return Configuration[EntityIdFieldKey]; }
			set { Configuration[EntityIdFieldKey] = value; }
		}

		#endregion

        #region Special SQL specific properties to prevent SQL Injection

	    private string originalUnsafeSql;
        //private Dictionary<string, string> sqlParams = new Dictionary<string, string>();
	    public const string ExtractedParamPrefix = "AutoExtractedParam";

        #endregion

        /// <summary>
		/// Initializes a new instance of the SqlDataSource class
		/// </summary>
		public SqlDataSource()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
			Configuration.Add(TitleFieldKey, EntityTitleDefaultColumnName);
			Configuration.Add(EntityIdFieldKey, EntityIdDefaultColumnName);
			Configuration.Add(ContentTypeKey, "[Settings:ContentType||SqlData]");
			Configuration.Add(SelectCommandKey, "[Settings:SelectCommand]");
			Configuration.Add(ConnectionStringKey, ConnectionStringDefault);
			Configuration.Add(ConnectionStringNameKey, "[Settings:ConnectionStringName]");

            CacheRelevantConfigurations = new[] { ContentTypeKey, SelectCommandKey, ConnectionStringKey, ConnectionStringNameKey };
        }

		/// <summary>
		/// Initializes a new instance of the SqlDataSource class
		/// </summary>
		public SqlDataSource(string connectionString, string selectCommand, string contentType, string entityIdField = null, string titleField = null)
			: this()
		{
			ConnectionString = connectionString;
			SelectCommand = selectCommand;
			ContentType = contentType;
			EntityIdField = entityIdField ?? EntityIdDefaultColumnName;
			TitleField = titleField ?? EntityTitleDefaultColumnName;
		}


	    protected internal override void EnsureConfigurationIsLoaded()
	    {
	        if (_configurationIsLoaded)
	            return;

            // Protect ourselves against SQL injection:
            // this is almost the same code as in the tokenizer, just replacing all tokens with an @param# syntax
            // and adding these @params to the collection of configurations
            var Tokenizer = Tokens.TokenReplace.Tokenizer;
	        
            // Before we process the Select-Command, we must get it (by default it's just a token!)
	        if (SelectCommand.StartsWith("[Settings"))
	        {
                var tempList = new Dictionary<string, string>();
                tempList.Add("one", SelectCommand);
                ConfigurationProvider.LoadConfiguration(tempList, null, 0); // load, but make sure no recursions to prevent pre-filling parameters
	            SelectCommand = tempList["one"];
	        }
            var sourceText = SelectCommand;
            var ParamNumber = 1;
	        var additionalParams = new List<string>();
            var Result = new StringBuilder();
            var charProgress = 0;
            var matches = Tokenizer.Matches(sourceText);
            if (matches.Count > 0)
            {
                foreach (Match curMatch in matches)
                {
                    // Get characters before the first match
                    if (curMatch.Index > charProgress)
                        Result.Append(sourceText.Substring(charProgress, curMatch.Index - charProgress));
                    charProgress = curMatch.Index + curMatch.Length;

                    var paramName = "@" + ExtractedParamPrefix + (ParamNumber++);
                    Result.Append(paramName);
                    Configuration.Add(paramName, curMatch.ToString());

                    // add name to list for caching-key
                    additionalParams.Add(paramName);
                }

                // attach the rest of the text (after the last match)
                Result.Append(sourceText.Substring(charProgress));

                // Ready to finish, but first, ensure repeating if desired
                SelectCommand = Result.ToString();
            }
	        CacheRelevantConfigurations = CacheRelevantConfigurations.Concat(additionalParams).ToArray();

	        base.EnsureConfigurationIsLoaded();
	    }

        //private IDictionary<int, IEntity> GetEntities()
        //{
        //    return GetList().ToDictionary(e => e.EntityId, e => e);
        //}

	    private IEnumerable<IEntity> GetList()
		{
			EnsureConfigurationIsLoaded();

            // Check if SQL contains forbidden terms
            if(ForbiddenTermsInSelect.IsMatch(SelectCommand))
                throw new System.InvalidOperationException("Found forbidden words in the select-command. Cannot continue.");

	        var list = new List<IEntity>(); // Dictionary<int, IEntity>();

			// Load ConnectionString by Name (if specified)
			if (!string.IsNullOrEmpty(ConnectionStringName) && (string.IsNullOrEmpty(ConnectionString) || ConnectionString == ConnectionStringDefault))
				ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;

			using (var connection = new SqlConnection(ConnectionString))
			{
				var command = new SqlCommand(SelectCommand, connection);

                // Add all items in Configuration starting with an @, as this should be an SQL parameter
				foreach (var sqlParameter in Configuration.Where(k => k.Key.StartsWith("@"))) 
					command.Parameters.AddWithValue(sqlParameter.Key, sqlParameter.Value);

				connection.Open();
				var reader = command.ExecuteReader();

				try
				{
					#region Get the SQL Column List and validate it
					var columNames = new string[reader.FieldCount];
					for (var i = 0; i < reader.FieldCount; i++)
						columNames[i] = reader.GetName(i);

					if (!columNames.Contains(EntityIdField))
						throw new Exception(string.Format("SQL Result doesn't contain an EntityId Column with Name \"{0}\". Ideally use something like Select ID As EntityId...", EntityIdField));
					if (!columNames.Contains(TitleField))
                        throw new Exception(string.Format("SQL Result doesn't contain an EntityTitle Column with Name \"{0}\". Ideally use something like Select FullName As EntityTitle...", TitleField));
					#endregion

					#region Read all Rows from SQL Server
					while (reader.Read())
					{
						var entityId = Convert.ToInt32(reader[EntityIdField]);
						var values = columNames.Where(c => c != EntityIdField).ToDictionary(c => c, c => reader[c]);
						var entity = new Data.Entity(entityId, ContentType, values, TitleField);
					    list.Add(entity);
					    //_entities.Add(entityId, entity);
					}
					#endregion
				}
				finally
				{
					reader.Close();
				}
			}

			return list;
		}
	}
}