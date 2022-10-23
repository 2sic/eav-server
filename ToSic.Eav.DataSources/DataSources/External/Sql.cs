using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Lib.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Provide Entities from a SQL Server
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    [VisualQuery(
        NiceName = "SQL Data",
        UiHint = "Get data from a database using SQL",
        Icon = Icons.FormDyn,
        Type = DataSourceType.Source,
        GlobalName = "ToSic.Eav.DataSources.Sql, ToSic.Eav.DataSources",
        DynamicOut = false,
        ExpectsDataOfType = "c76901b5-0345-4866-9fa3-6208de7f8543",
        PreviousNames = new []
            {
                "ToSic.Eav.DataSources.SqlDataSource, ToSic.Eav.DataSources"
            },
        HelpLink = "https://r.2sxc.org/DsSql")]

	public class Sql : ExternalData
	{
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.ExtSql";

        // Note: of the standard SQL-terms, I will only allow exec|execute|select
        // Everything else shouldn't be allowed
        [PrivateApi]
        public static Regex ForbiddenTermsInSelect = new Regex(@"(;|\s|^)+(insert|update|delete|create|alter|drop|rename|truncate|backup|restore)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		#region Configuration-properties
        [PrivateApi] protected const string TitleFieldKey = "TitleField";
        [PrivateApi] protected const string EntityIdFieldKey = "EntityIdField";
        [PrivateApi] protected const string ContentTypeKey = "ContentType";
        [PrivateApi] protected const string SelectCommandKey = "SelectCommand";
        [PrivateApi] protected const string ConnectionStringKey = "ConnectionString";
        [PrivateApi] protected const string ConnectionStringNameKey = "ConnectionStringName";
        [PrivateApi] protected const string ConnectionStringDefault = "[Settings:ConnectionString]";

		/// <summary>
		/// Name of the ConnectionString in the Application.Config to use
		/// </summary>
		public string ConnectionStringName
		{
			get => Configuration[ConnectionStringNameKey];
		    set => Configuration[ConnectionStringNameKey] = value;
		}

		/// <summary>
		/// ConnectionString to the DB
		/// </summary>
		public string ConnectionString
		{
			get => Configuration[ConnectionStringKey];
		    set => Configuration[ConnectionStringKey] = value;
		}

		/// <summary>
		/// SQL Command for selecting data.
		/// </summary>
		public string SelectCommand
		{
			get => Configuration[SelectCommandKey];
		    set => Configuration[SelectCommandKey] = value;
		}

		/// <summary>
		/// Name of the ContentType which we'll pretend the items have.
		/// </summary>
		public string ContentType
		{
			get => Configuration[ContentTypeKey];
		    set => Configuration[ContentTypeKey] = value;
		}

		/// <summary>
		/// Name of the Title Attribute of the Source DataTable
		/// </summary>
		public string TitleField
		{
			get => Configuration[TitleFieldKey];
		    set => Configuration[TitleFieldKey] = value;
		}

		/// <summary>
		/// Name of the Column used as EntityId
		/// </summary>
		public string EntityIdField
		{
			get => Configuration[EntityIdFieldKey];
		    set => Configuration[EntityIdFieldKey] = value;
		}

		#endregion

        #region Special SQL specific properties to prevent SQL Injection
        [PrivateApi]
	    public const string ExtractedParamPrefix = "AutoExtractedParam";

		#endregion

        #region Error Constants

        public const string ErrorTitleForbiddenSql = "Forbidden SQL words";


		#endregion

		// Important: This constructor must come BEFORE the other constructors
		// because it is the one which the .net Core DI should use!
		/// <summary>
		/// Initializes a new instance of the SqlDataSource class
		/// </summary>
		[PrivateApi]
		public Sql()
		{
			Provide(GetList);
		    ConfigMask(TitleFieldKey, "[Settings:EntityTitleField||" + Attributes.EntityFieldTitle + "]");
		    ConfigMask(EntityIdFieldKey, "[Settings:EntityIdField||" + Attributes.EntityFieldId + "]");

		    ConfigMask(ContentTypeKey, "[Settings:ContentType||SqlData]");
		    ConfigMask(SelectCommandKey, "[Settings:SelectCommand]");
		    ConfigMask(ConnectionStringKey, ConnectionStringDefault);
		    ConfigMask(ConnectionStringNameKey, "[Settings:ConnectionStringName]");
        }

		/// <summary>
		/// Initializes a new instance of the SqlDataSource class
		/// </summary>
		/// <param name="connectionString">Connection String to the DB</param>
		/// <param name="selectCommand">SQL Query</param>
		/// <param name="contentType">Name of virtual content-type we'll return</param>
		/// <param name="entityIdField">ID-field in the DB to use</param>
		/// <param name="titleField">Title-field in the DB to use</param>
		/// <remarks>
		/// Before 12.09 this was a constructor, but couldn't actually work because it wasn't DI compatible any more.
        /// So we changed it, assuming it wasn't actually used as a constructor before, but only in test code. Marked as private for now
        /// </remarks>
        [PrivateApi]
        public Sql Setup(string connectionString, string selectCommand, string contentType, string entityIdField = null, string titleField = null)
		{
			ConnectionString = connectionString;
			SelectCommand = selectCommand;
			ContentType = contentType;
		    EntityIdField = entityIdField ?? Attributes.EntityFieldId;
			TitleField = titleField ?? Attributes.EntityFieldTitle;
            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Replace original EnsureConfigurationIsLoaded to handle the SQL in a special way
        /// </summary>
        [PrivateApi]
	    internal void CustomConfigurationParse()
	    {
	        if (Configuration.IsParsed)
	            return;

            // Protect ourselves against SQL injection:
            // this is almost the same code as in the tokenizer, just replacing all tokens with an @param# syntax
            // and adding these @params to the collection of configurations
            var tokenizer = TokenReplace.Tokenizer;

            // Before we process the Select-Command, we must get it (by default it's just a token!)
	        if (SelectCommand.StartsWith("[Settings"))
	        {
	            var tempList = Configuration.LookUpEngine.LookUp(
                    new Dictionary<string, string> { { "one", SelectCommand } },
                    null, 0); // load, but make sure no recursions to prevent pre-filling parameters
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
                    Configuration.Values.Add(paramName, curMatch.ToString());

                    // add name to list for caching-key
                    additionalParams.Add(paramName);
                }

                // attach the rest of the text (after the last match)
                result.Append(sourceText.Substring(charProgress));

                // Ready to finish, but first, ensure repeating if desired
                SelectCommand = result.ToString();
            }
	        CacheRelevantConfigurations = CacheRelevantConfigurations.Concat(additionalParams).ToList();

            Configuration.Parse();
        }


	    private ImmutableArray<IEntity> GetList()
		{
            CustomConfigurationParse();

            Log.A($"get from sql:{SelectCommand}");

            // Check if SQL contains forbidden terms
            if (ForbiddenTermsInSelect.IsMatch(SelectCommand))
                return ErrorHandler.CreateErrorList(source: this, title: ErrorTitleForbiddenSql,
                    message: $"{GetType().Name} - Found forbidden words in the select-command. Cannot continue.");


            // Load ConnectionString by Name (if specified)
			if (!string.IsNullOrEmpty(ConnectionStringName) && (string.IsNullOrEmpty(ConnectionString) || ConnectionString == ConnectionStringDefault))
			    try
			    {
			        ConnectionString = global::System.Configuration.ConfigurationManager
                        .ConnectionStrings[ConnectionStringName].ConnectionString;
			    }
			    catch(Exception ex)
                {
                    return ErrorHandler.CreateErrorList(source: this, exception: ex,
                        title: "Can't find Connection String Name",
                        message: "The specified connection string-name doesn't seem to exist. For security reasons it's not included in this message.");
			    }

            // make sure we have one - often it's empty, if the query hasn't been configured yet
            if (string.IsNullOrWhiteSpace(ConnectionString))
                return ErrorHandler.CreateErrorList(source: this, title: "Connection Problem",
                    message: "The ConnectionString property is empty / has not been initialized");

			// The content type returned in this query
			var contentTypeToUse = DataBuilder.Type(ContentType);

			var list = new List<IEntity>();
            using (var connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
                // create a fake transaction, to ensure no changes can be made
			    using (var trans = connection.BeginTransaction())
			    {
				    var command = new SqlCommand(SelectCommand, connection, trans);

                    // Add all items in Configuration starting with an @, as this should be an SQL parameter
				    foreach (var sqlParameter in Configuration.Values.Where(k => k.Key.StartsWith("@")))
					    command.Parameters.AddWithValue(sqlParameter.Key, sqlParameter.Value);

                    SqlDataReader reader;
                    try
                    {
                        reader = command.ExecuteReader();
                    }
                    catch(Exception ex)
                    {
                        return ErrorHandler.CreateErrorList(source: this, exception: ex,
                            title: "Can't read from Database",
                            message: "Something failed trying to read from the Database.");
                    }

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
			            Log.A($"will used '{casedEntityId}' as entity field (null if not found)");

			            // try alternate casing - new: just take first column if the defined one isn't found - worst case it doesn't have a title
			            if (!columNames.Contains(casedTitle))
			                casedTitle = columNames.FirstOrDefault(c =>
			                                 string.Equals(c, casedTitle, StringComparison.InvariantCultureIgnoreCase))
			                             ?? columNames.FirstOrDefault();
			            Log.A($"will use '{casedTitle}' as title field");

                        #endregion

                        #region Read all Rows from SQL Server

                        // apparently SQL could return the same column name - which would cause problems - so distinct them first
                        var columnsToUse = columNames.Where(c => c != casedEntityId).Distinct().ToList();
			            while (reader.Read())
			            {
			                var entityId = casedEntityId == null ? 0 : global::System.Convert.ToInt32(reader[casedEntityId]);
			                var values = columnsToUse.ToDictionary(c => c, c =>
                            {
								// This conversion is important, because the DB uses a different kind of null, which would cause trouble
								var value = reader[c];
                                return Convert.IsDBNull(value) ? null : value;
                            });
			                var entity = new Entity(Constants.TransientAppId, entityId, contentTypeToUse, values, casedTitle);
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

		    Log.A($"found:{list.Count}");
			return list.ToImmutableArray();
		}
	}
}
