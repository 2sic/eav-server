using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Internal.Environment;
using static System.StringComparison;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <summary>
/// DataSource for importing/reading CSV files. 
/// </summary>
/// <remarks>
/// * Renamed from `CsvDataSource` to `Csv` in v15.06 for consistency. Believe this should not affect anybody.
/// </remarks>
[PublicApi]
[VisualQuery(
    NiceName = "CSV Data",
    UiHint = "Load data from a CSV file",
    Icon = DataSourceIcons.Description,
    Type = DataSourceType.Source,
    NameId = "ToSic.Eav.DataSources.CsvDataSource, ToSic.Eav.DataSources",
    DynamicOut = false,
    ConfigurationType = "|Config ToSic.Eav.DataSources.CsvDataSource",
    HelpLink = "https://go.2sxc.org/DsCsv")]
public class Csv : CustomDataSourceAdvanced
{
    #region Known errors
    [PrivateApi]
    internal const string ErrorIdNaN = "ID is not a number";

    #endregion

    /// <summary>
    /// Path to the CSV file, relative to the website root
    /// </summary>
    [Configuration]
    public string FilePath
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// Full path to the CSV file. 
    /// </summary>
    private string GetServerPath(string csvPath)
    {
        var l = Log.Fn<string>($"{nameof(csvPath)}: {csvPath}");
        // Handle cases where it's a "file:72"
        if (ValueConverterBase.CouldBeReference(csvPath))
        {
            csvPath = _serverPaths.FullPathOfReference(csvPath);
            return l.Return(csvPath, $"seems to be a ref, now: {csvPath}");
        }

        l.A("Doesn't seem to be a reference, will use as is");

        // if it's a full path, use that, otherwise do map-path assuming it must be in the app
        // this is for backward compatibility, because old samples used "[App:Path]/something.csv" which returns a relative path
        var result = csvPath.IsPathWithDriveOrNetwork()
            ? csvPath
            : _serverPaths.FullContentPath(csvPath);
        return l.Return(result, $"not a ref, now: {result}");
    }


    /// <summary>
    /// Delimiter character in the CSV, usually a ',' or ';' but could also be a tab or something. Default is tab.
    /// </summary>
    [Configuration(Fallback = "\t")]
    public string Delimiter
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }


    /// <summary>
    /// Name of the content type which the imported entities have. This is fake, but may be necessary for later filtering of the types.
    /// Defaults to "CSV"
    /// </summary>
    /// <remarks>
    /// * Before v15.03 it defaulted to "Anonymous"
    /// </remarks>
    [Configuration(Fallback = "CSV")]
    public string ContentType
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// Column in the CSV which contains the ID. 
    /// </summary>
    [Configuration]
    public string IdColumnName
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }


    /// <summary>
    /// The CSV column containing the title of the item - for dropdowns etc. and the EntityTitle property. 
    /// </summary>
    [Configuration]
    public string TitleColumnName
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }


    [PrivateApi]
    public Csv(MyServices services, IDataFactory dataFactory, IUser user, IServerPaths serverPaths)
        : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Csv", connect: [user, serverPaths, dataFactory])
    {
        _user = user;
        _serverPaths = serverPaths;
        _dataFactory = dataFactory;
        ProvideOut(GetList);
    }
    private readonly IUser _user;
    private readonly IServerPaths _serverPaths;
    private readonly IDataFactory _dataFactory;


    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();

        // Collect parameters here, so we don't trigger logs on each access of each property
        var delimiter = Delimiter;
        var idColumnName = IdColumnName;
        var titleColumnName = TitleColumnName;

        var entityList = new List<IEntity>();

        var csvPath = GetServerPath(FilePath);
        l.A($"CSV path:'{csvPath}', delimiter:'{delimiter}'");

        if (string.IsNullOrWhiteSpace(csvPath))
            return l.ReturnAsError(Error.Create(title: "No Path Given", message: "There was no path for loading the CSV file."));

        var pathPart = Path.GetDirectoryName(csvPath);
        if (!Directory.Exists(pathPart))
        {
            l.A($"Didn't find path '{pathPart}'");
            return l.ReturnAsError(Error.Create(title: "Path not found",
                message: _user?.IsSystemAdmin == true
                    ? $"Path for Super User only: '{pathPart}'"
                    : "The path given was not found. For security reasons it's not included in the message. You'll find it in the Insights."));
        }

        if (!File.Exists(csvPath))
            return l.ReturnAsError(Error.Create(title: "CSV File Not Found",
                message: _user?.IsSystemAdmin == true
                    ? $"Path for Super User only: '{csvPath}'"
                    : "For security reasons the path isn't mentioned here. You'll find it in the Insights."));

        const string commonErrorsIdTitle =
            "A common mistake is to use the wrong delimiter (comma / semi-colon) in which case this may also fail. ";

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim
        };

        using (var stream = new StreamReader(csvPath))
        using (var parser = new CsvParser(stream, config))
        {
            const int idColumnNotDetermined = -999;
            var idColumnIndex = idColumnNotDetermined;
            string titleColName;


            // Parse header - must happen after the first read
            parser.Read();
            var headers = parser.Record ?? [];

            // If we should find the Column...
            if (!string.IsNullOrEmpty(idColumnName))
            {
                // on first round, check the headers fields
                // Try to find - first case-sensitive, then insensitive
                idColumnIndex = Array.FindIndex(headers, name => name == idColumnName);
                if (idColumnIndex == -1)
                    idColumnIndex = Array.FindIndex(headers,
                        name => name.Equals(idColumnName, InvariantCultureIgnoreCase));
                if (idColumnIndex == -1)
                    return l.ReturnAsError(Error.Create(title: "ID Column not found",
                        message: $"ID column '{idColumnName}' specified cannot be found in the file. " +
                                 $"The Headers: '{string.Join(",", headers)}'. " +
                                 $"{commonErrorsIdTitle}"));
            }

            if (string.IsNullOrEmpty(titleColumnName))
                titleColName = headers[0];
            else
            {
                // The following is a little bit complicated, but it checks that the title specified exists
                titleColName = headers.FirstOrDefault(colName => colName == titleColumnName)
                               ?? headers.FirstOrDefault(colName =>
                                   colName.Equals(titleColumnName, InvariantCultureIgnoreCase));
                if (titleColName == null)
                    return l.ReturnAsError(Error.Create(title: "Title column not found",
                        message: $"Title column '{titleColumnName}' cannot be found in the file. " +
                                 $"The Headers: '{string.Join(",", headers)}'. " +
                                 $"{commonErrorsIdTitle}"));
            }

            var csvFactory = DataFactory.SpawnNew(new()
            {
                AppId = Constants.TransientAppId,
                TitleField = titleColName,
                TypeName = ContentType,
            });

            // Parse data
            while (parser.Read())
            {
                var fields = parser.Record ?? [];

                int entityId;
                // No ID column specified, so use the row number
                if (string.IsNullOrEmpty(idColumnName))
                    entityId = parser.Row;
                // check if id can be parsed from the current row
                else if (!int.TryParse(fields[idColumnIndex], out entityId))
                    return l.ReturnAsError(Error.Create(title: ErrorIdNaN,
                        message:
                        $"Row {parser.Row}: ID field '{headers[idColumnIndex]}' cannot be parsed to int. Value was '{fields[idColumnIndex]}'."));

                var entityValues = new Dictionary<string, object>();
                for (var i = 0; i < headers.Length; i++)
                    entityValues.Add(headers[i], (i < fields.Length) ? fields[i] : null);

                entityList.Add(csvFactory.Create(values: entityValues, id: entityId));
            }
        }

        return l.Return(entityList.ToImmutableList(), $"{entityList.Count}");
    }
}