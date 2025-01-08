#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using System.Text;
using System.Text.RegularExpressions;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources.Internal;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;
using static System.StringComparison;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <summary>
/// Provide Entities from a SQL Server
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "SQL Data",
    UiHint = "Get data from a database using SQL",
    Icon = DataSourceIcons.FormDyn,
    Type = DataSourceType.Source,
    NameId = "ToSic.Eav.DataSources.Sql, ToSic.Eav.DataSources",
    DynamicOut = false,
    ConfigurationType = "c76901b5-0345-4866-9fa3-6208de7f8543",
    NameIds =
    [
        "ToSic.Eav.DataSources.SqlDataSource, ToSic.Eav.DataSources"
    ],
    HelpLink = "https://go.2sxc.org/DsSql")]

public class Sql : CustomDataSourceAdvanced
{
    // Note: of the standard SQL-terms, I will only allow exec|execute|select
    // Everything else shouldn't be allowed
    internal static Regex ForbiddenTermsInSelect = new(@"(;|\s|^)+(insert|update|delete|create|alter|drop|rename|truncate|backup|restore|sp_executesql)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    #region Configuration-properties

    /// <summary>
    /// Name of the ConnectionString in the Application.Config to use
    /// </summary>
    [Configuration]
    public string ConnectionStringName
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// ConnectionString to the DB
    /// </summary>
    [Configuration]
    public string ConnectionString
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// SQL Command for selecting data.
    /// </summary>
    [Configuration]
    public string SelectCommand
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// Name of the ContentType which we'll pretend the items have.
    /// </summary>
    [Configuration(Fallback = "SqlData")]
    public string ContentType
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// Name of the Title Attribute of the Source DataTable
    /// </summary>
    [Configuration(Field = "EntityTitleField", Fallback = Attributes.EntityFieldTitle)]
    public string TitleField
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// Name of the Column used as EntityId
    /// </summary>
    [Configuration(Fallback = Attributes.EntityFieldId)]
    public string EntityIdField
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    #endregion

    /// <summary>
    /// Special SQL specific properties to prevent SQL Injection
    /// </summary>
    internal const string ExtractedParamPrefix = "AutoExtractedParam";

    /// <summary>
    /// Error Constants
    /// </summary>
    internal const string ErrorTitleForbiddenSql = "Forbidden SQL words";


    #region Constructor

    [PrivateApi]
    public new class MyServices: MyServicesBase<CustomDataSourceAdvanced.MyServices>
    {
        public SqlPlatformInfo SqlPlatformInfo { get; }
        public MyServices(SqlPlatformInfo sqlPlatformInfo, CustomDataSourceAdvanced.MyServices parentServices): base(parentServices)
        {
            ConnectLogs([
                SqlPlatformInfo = sqlPlatformInfo
            ]);
        }
    }

    // Important: This constructor must come BEFORE the other constructors
    // because it is the one which the .net Core DI should use!
    /// <summary>
    /// Initializes a new instance of the SqlDataSource class
    /// </summary>
    [PrivateApi]
    public Sql(MyServices services, IDataFactory dataFactory) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.ExtSql")
    {
        ConnectLogs([
            _dataFactory = dataFactory
        ]);
        SqlServices = services;
        ProvideOut(GetList);
    }
    [PrivateApi] protected readonly MyServices SqlServices;
    private readonly IDataFactory _dataFactory;

    #endregion

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
    internal Sql Setup(string connectionString, string selectCommand, string contentType, string entityIdField = null, string titleField = null)
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
        var selectSql = SelectCommand;
        if (selectSql.StartsWith("[" + DataSourceConstants.MyConfigurationSourceName, InvariantCultureIgnoreCase))
        {
            var tempList = Configuration.LookUpEngine.LookUp(
                new Dictionary<string, string> { { "one", selectSql } },
                overrides: null,
                depth: 0 // load, but make sure no recursions to prevent pre-filling parameters
            );
            selectSql = tempList["one"];
        }
        var paramNumber = 1;
        var additionalParams = new List<string>();
        var result = new StringBuilder();
        var charProgress = 0;
        var matches = tokenizer.Matches(selectSql);
        if (matches.Count == 0)
            SelectCommand = selectSql;
        else
        {
            foreach (Match curMatch in matches)
            {
                // Get characters before the first match
                if (curMatch.Index > charProgress)
                    result.Append(selectSql.Substring(charProgress, curMatch.Index - charProgress));
                charProgress = curMatch.Index + curMatch.Length;

                var paramName = $"@{ExtractedParamPrefix}{paramNumber++}";
                result.Append(paramName);
                ConfigMask(paramName, curMatch.ToString());

                // add name to list for caching-key
                additionalParams.Add(paramName);
            }

            // attach the rest of the text (after the last match)
            result.Append(selectSql.Substring(charProgress));

            // Ready to finish, but first, ensure repeating if desired
            SelectCommand = result.ToString();
        }

        // Add all additional tokens/parameters to the cache-keys, so any change in these values will use a different cache
        CacheRelevantConfigurations.AddRange(additionalParams);

        Configuration.Parse();
    }


    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        CustomConfigurationParse();

        var selectSql = SelectCommand;
        l.A($"get from sql:{selectSql}");

        // Check if SQL contains forbidden terms
        if (ForbiddenTermsInSelect.IsMatch(selectSql))
            return l.ReturnAsError(Error.Create(source: this, title: ErrorTitleForbiddenSql,
                message: $"{GetType().Name} - Found forbidden words in the select-command. Cannot continue."));


        // Load ConnectionString by Name (if specified)
        var conStringNameRaw = ConnectionStringName;
        if (!string.IsNullOrEmpty(conStringNameRaw) && string.IsNullOrEmpty(ConnectionString))
            try
            {
                var conStringName = conStringNameRaw.IsEmptyOrWs() ||
                                    conStringNameRaw.EqualsInsensitive(SqlPlatformInfo.DefaultConnectionPlaceholder)
                    ? SqlServices.SqlPlatformInfo.DefaultConnectionStringName
                    : conStringNameRaw;

                ConnectionString = SqlServices.SqlPlatformInfo.FindConnectionString(conStringName);
            }
            catch(Exception ex)
            {
                return l.ReturnAsError(Error.Create(source: this, exception: ex,
                    title: "Can't find Connection String Name",
                    message: "The specified connection string-name doesn't seem to exist. For security reasons it's not included in this message."));
            }

        // make sure we have one - often it's empty, if the query hasn't been configured yet
        if (string.IsNullOrWhiteSpace(ConnectionString))
            return l.ReturnAsError(Error.Create(source: this, title: "Connection Problem",
                message: "The ConnectionString property is empty / has not been initialized"));

        var list = new List<IEntity>();
        using (var connection = new SqlConnection(ConnectionString))
        {
            connection.Open();
            // create a fake transaction, to ensure no changes can be made
            using (var trans = connection.BeginTransaction())
            {
                var command = new SqlCommand(selectSql, connection, trans);

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
                    return l.ReturnAsError(Error.Create(source: this, exception: ex,
                        title: "Can't read from Database",
                        message: "Something failed trying to read from the Database."));
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
                            string.Equals(c, casedEntityId, InvariantCultureIgnoreCase));
                    l.A($"will used '{casedEntityId}' as entity field (null if not found)");

                    // try alternate casing - new: just take first column if the defined one isn't found - worst case it doesn't have a title
                    if (!columNames.Contains(casedTitle))
                        casedTitle = columNames.FirstOrDefault(c =>
                                         string.Equals(c, casedTitle, InvariantCultureIgnoreCase))
                                     ?? columNames.FirstOrDefault();
                    l.A($"will use '{casedTitle}' as title field");

                    var sqlFactory = _dataFactory.New(options: new()
                    {
                        AppId = Constants.TransientAppId,
                        TitleField = casedTitle,
                        TypeName = ContentType,
                    });

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
                        var entity = sqlFactory.Create(values, id: entityId);
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

        return l.Return(list.ToImmutableList(), $"found:{list.Count}");
    }
}