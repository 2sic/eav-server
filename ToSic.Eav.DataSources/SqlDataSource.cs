using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToSic.Eav.DataSources.Attributes;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Provide Entities from a SQL Server
    /// </summary>
    [PipelineDesigner]
    [DataSourceProperties(Type = DataSourceType.Source, DynamicOut = false,
        Icon = "database",
        ExpectsDataOfType = "c76901b5-0345-4866-9fa3-6208de7f8543",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-SqlDataSource")]

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
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
			Configuration.Add(TitleFieldKey, "[Settings:EntityTitleField||" + Constants.EntityFieldTitle + "]");

            Configuration.Add(EntityIdFieldKey, "[Settings:EntityIdField||" + Constants.EntityFieldId + "]");

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

        /// <inheritdoc />
        /// <summary>
        /// Replace original EnsureConfigurationIsLoaded to handle the SQL in a special way
        /// </summary>
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
                throw new InvalidOperationException($"{GetType().Name} - Found forbidden words in the select-command. Cannot continue.");

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

            // make sure we have one - often it's empty, if the query hasn't been configured yet
            if (string.IsNullOrWhiteSpace(ConnectionString))
		        throw new Exception($"{GetType().Name} - The ConnectionString property has not been initialized");

            using (var connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
                // create a fake transaction, to ensure no changes can be made
			    using (var trans = connection.BeginTransaction())
			    {
				    var command = new SqlCommand(SelectCommand, connection, trans);

                    // Add all items in Configuration starting with an @, as this should be an SQL parameter
				    foreach (var sqlParameter in Configuration.Where(k => k.Key.StartsWith("@"))) 
					    command.Parameters.AddWithValue(sqlParameter.Key, sqlParameter.Value);

			        var reader = command.ExecuteReader();


			        var casedTitle = TitleField;
			        var casedEntityId = EntityIdField;
			        try
			        {
			            #region Get the SQL Column List and validate it

			            var columNames = new string[reader.FieldCount];
			            for (var i = 0; i < reader.FieldCount; i++)
			                columNames[i] = reader.GetName(i);

			            // try alternate casing - will result in null if not found (handled later on)
			            if (!columNames.Contains(casedEntityId))
			                casedEntityId = columNames.FirstOrDefault(c =>
			                    string.Equals(c, casedEntityId, StringComparison.InvariantCultureIgnoreCase));
			            Log.Add($"will used '{casedEntityId}' as entity field (null if not found)");

			            // try alternate casing - new: just take first column if the defined one isn't found - worst case it doesn't have a title
			            if (!columNames.Contains(casedTitle))
			                casedTitle = columNames.FirstOrDefault(c =>
			                                 string.Equals(c, casedTitle, StringComparison.InvariantCultureIgnoreCase))
			                             ?? columNames.FirstOrDefault();
			            Log.Add($"will use '{casedTitle}' as title field");

			            #endregion

			            #region Read all Rows from SQL Server

			            while (reader.Read())
			            {
			                var entityId = casedEntityId == null ? 0 : Convert.ToInt32(reader[casedEntityId]);
			                var values = columNames.Where(c => c != casedEntityId).ToDictionary(c => c, c => reader[c]);
			                var entity = new Data.Entity(Constants.TransientAppId, entityId, ContentType, values, casedTitle);
			                list.Add(entity);
			            }

			            #endregion
			        }
			        finally
			        {
			            reader.Close();
			            // cause transaction rollback, in case there was a change made by naughty sql
			            trans.Rollback();
			        }
			    }
			}

		    Log.Add($"found:{list.Count}");
			return list;
		}
	}
}