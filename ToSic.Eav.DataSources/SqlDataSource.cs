using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Types.Attributes;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Provide Entities from a SQL Server
	/// </summary>
	[PipelineDesigner]
    [ExpectsDataOfType(ContentTypes.ConfigSqlDataSource.StaticTypeName)]
	public class SqlDataSource : ExternalDataDataSource
	{
        // Note: of the standard SQL-terms, I will only allow exec|execute|select
        // Everything else shouldn't be allowed
        public static Regex ForbiddenTermsInSelect = new Regex(@"(;|\s|^)+(insert|update|delete|create|alter|drop|rename|truncate|backup|restore)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		#region Configuration-properties
		protected const string TitleFieldKey = "TitleField";
		protected const string EntityIdFieldKey = "EntityIdField";
		protected const string ContentTypeKey = "ContentType";
		protected const string SelectCommandKey = "SelectCommand";
		protected const string ConnectionStringKey = "ConnectionString";
		protected const string ConnectionStringNameKey = "ConnectionStringName";
		protected const string ConnectionStringDefault = "[Settings:ConnectionString]";

		/// <summary>
		/// Gets or sets the name of the ConnectionString in the Application.Config to use
		/// </summary>
		public string ConnectionStringName
		{
			get => Configuration[ConnectionStringNameKey];
		    set => Configuration[ConnectionStringNameKey] = value;
		}

		/// <summary>
		/// Gets or sets the ConnectionString
		/// </summary>
		public string ConnectionString
		{
			get => Configuration[ConnectionStringKey];
		    set => Configuration[ConnectionStringKey] = value;
		}

		/// <summary>
		/// Gets or sets the SQL Command
		/// </summary>
		public string SelectCommand
		{
			get => Configuration[SelectCommandKey];
		    set => Configuration[SelectCommandKey] = value;
		}

		/// <summary>
		/// Gets or sets the Name of the ContentType
		/// </summary>
		public string ContentType
		{
			get => Configuration[ContentTypeKey];
		    set => Configuration[ContentTypeKey] = value;
		}

		/// <summary>
		/// Gets or sets the Name of the Title Attribute of the Source DataTable
		/// </summary>
		public string TitleField
		{
			get => Configuration[TitleFieldKey];
		    set => Configuration[TitleFieldKey] = value;
		}

		/// <summary>
		/// Gets or sets the Name of the Column used as EntityId
		/// </summary>
		public string EntityIdField
		{
			get => Configuration[EntityIdFieldKey];
		    set => Configuration[EntityIdFieldKey] = value;
		}

		#endregion

        #region Special SQL specific properties to prevent SQL Injection

	    public const string ExtractedParamPrefix = "AutoExtractedParam";

        #endregion

        // Important: This constructor must come BEFORE the other constructors
        // because it is the one which the .net Core DI should use!
        /// <summary>
		/// Initializes a new instance of the SqlDataSource class
		/// </summary>
		public SqlDataSource()
		{
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
			Configuration.Add(TitleFieldKey, Constants.EntityFieldTitle);
		    Configuration.Add(EntityIdFieldKey, Constants.EntityFieldId);// EntityIdDefaultColumnName);
			Configuration.Add(ContentTypeKey, "[Settings:ContentType||SqlData]");
			Configuration.Add(SelectCommandKey, "[Settings:SelectCommand]");
			Configuration.Add(ConnectionStringKey, ConnectionStringDefault);
			Configuration.Add(ConnectionStringNameKey, "[Settings:ConnectionStringName]");

            CacheRelevantConfigurations = new[] { ContentTypeKey, SelectCommandKey, ConnectionStringKey, ConnectionStringNameKey };
        }

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the SqlDataSource class
		/// </summary>
		public SqlDataSource(string connectionString, string selectCommand, string contentType, string entityIdField = null, string titleField = null)
			: this()
		{
			ConnectionString = connectionString;
			SelectCommand = selectCommand;
			ContentType = contentType;
		    EntityIdField = entityIdField ?? Constants.EntityFieldId;// EntityIdDefaultColumnName;
			TitleField = titleField ?? Constants.EntityFieldTitle;
		}


	    protected internal override void EnsureConfigurationIsLoaded()
	    {
	        if (_configurationIsLoaded)
	            return;

            // Protect ourselves against SQL injection:
            // this is almost the same code as in the tokenizer, just replacing all tokens with an @param# syntax
            // and adding these @params to the collection of configurations
            var tokenizer = Tokens.TokenReplace.Tokenizer;
	        
            // Before we process the Select-Command, we must get it (by default it's just a token!)
	        if (SelectCommand.StartsWith("[Settings"))
	        {
	            var tempList = new Dictionary<string, string> {{"one", SelectCommand}};
	            ConfigurationProvider.LoadConfiguration(tempList, null, 0); // load, but make sure no recursions to prevent pre-filling parameters
	            SelectCommand = tempList["one"];
	        }
            var sourceText = SelectCommand;
            var paramNumber = 1;
	        var additionalParams = new List<string>();
            var result = new StringBuilder();
            var charProgress = 0;
            var matches = tokenizer.Matches(sourceText);
            if (matches.Count > 0)
            {
                foreach (Match curMatch in matches)
                {
                    // Get characters before the first match
                    if (curMatch.Index > charProgress)
                        result.Append(sourceText.Substring(charProgress, curMatch.Index - charProgress));
                    charProgress = curMatch.Index + curMatch.Length;

                    var paramName = "@" + ExtractedParamPrefix + (paramNumber++);
                    result.Append(paramName);
                    Configuration.Add(paramName, curMatch.ToString());

                    // add name to list for caching-key
                    additionalParams.Add(paramName);
                }

                // attach the rest of the text (after the last match)
                result.Append(sourceText.Substring(charProgress));

                // Ready to finish, but first, ensure repeating if desired
                SelectCommand = result.ToString();
            }
	        CacheRelevantConfigurations = CacheRelevantConfigurations.Concat(additionalParams).ToArray();

	        base.EnsureConfigurationIsLoaded();
	    }


	    private IEnumerable<IEntity> GetList()
		{
			EnsureConfigurationIsLoaded();

		    Log.Add($"get from sql:{SelectCommand}");

            // Check if SQL contains forbidden terms
            if(ForbiddenTermsInSelect.IsMatch(SelectCommand))
                throw new InvalidOperationException("Found forbidden words in the select-command. Cannot continue.");

	        var list = new List<IEntity>();

		    // Load ConnectionString by Name (if specified)
			if (!string.IsNullOrEmpty(ConnectionStringName) && (string.IsNullOrEmpty(ConnectionString) || ConnectionString == ConnectionStringDefault))
			    try
			    {
			        ConnectionString = System.Configuration.ConfigurationManager
                        .ConnectionStrings[ConnectionStringName].ConnectionString;
			    }
			    catch (Exception ex)
			    {
			        throw new Exception("error trying to load exception string", ex);
			    }

		    using (var connection = new SqlConnection(ConnectionString))
			{
				var command = new SqlCommand(SelectCommand, connection);

                // Add all items in Configuration starting with an @, as this should be an SQL parameter
				foreach (var sqlParameter in Configuration.Where(k => k.Key.StartsWith("@"))) 
					command.Parameters.AddWithValue(sqlParameter.Key, sqlParameter.Value);

				connection.Open();
				var reader = command.ExecuteReader();

			    var casedTitle = TitleField;
			    var casedEntityId = EntityIdField;
				try
				{
					#region Get the SQL Column List and validate it
					var columNames = new string[reader.FieldCount];
					for (var i = 0; i < reader.FieldCount; i++)
						columNames[i] = reader.GetName(i);

					if (!columNames.Contains(casedEntityId))
					{
                        // try alternate casing
                        casedEntityId = columNames.FirstOrDefault(c => string.Equals(c, casedEntityId, StringComparison.InvariantCultureIgnoreCase));
					    if (casedEntityId == null)
					        throw new Exception(
					            $"SQL Result doesn't contain an EntityId Column with Name '{EntityIdField}'. " +
					            "Ideally use something like Select ID As EntityId...");
					}

				    if (!columNames.Contains(casedTitle))
				    {
                        // try alternate casing
				        casedTitle = columNames.FirstOrDefault(c => string.Equals(c, casedTitle, StringComparison.InvariantCultureIgnoreCase));
				        if (casedTitle == null)
				            throw new Exception(
				                $"SQL Result doesn't contain an EntityTitle Column with Name '{TitleField}'. " +
				                "Ideally use something like Select FullName As EntityTitle...");
				    }

				    #endregion

					#region Read all Rows from SQL Server
					while (reader.Read())
					{
						var entityId = Convert.ToInt32(reader[casedEntityId]);
						var values = columNames.Where(c => c != casedEntityId).ToDictionary(c => c, c => reader[c]);
						var entity = new Data.Entity(Constants.TransientAppId, entityId, ContentType, values, casedTitle);
					    list.Add(entity);
					}
					#endregion
				}
				finally
				{
					reader.Close();
				}
			}

		    Log.Add($"found:{list.Count}");
			return list;
		}
	}
}