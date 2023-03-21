using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Logging;
using ToSic.Eav.Run;
using ToSic.Lib.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// DataSource for importing/reading CSV files. 
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    [VisualQuery(
        NiceName = "CSV Data",
        UiHint = "Load data from a CSV file",
        Icon = Icons.Description,
        Type = DataSourceType.Source,
        GlobalName = "ToSic.Eav.DataSources.CsvDataSource, ToSic.Eav.DataSources",
        DynamicOut = false,
        ConfigurationType = "|Config ToSic.Eav.DataSources.CsvDataSource",
        HelpLink = "https://r.2sxc.org/DsCsv")]
    public class CsvDataSource : CustomDataSourceAdvanced
    {
        #region Known errors
        [PrivateApi]
        public const string ErrorIdNaN = "ID is not a number";

        #endregion

        /// <summary>
        /// Path to the CSV file, relative to the website root
        /// </summary>
        [Configuration]
        public string FilePath
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// Full path to the CSV file. 
        /// </summary>
        private string GetServerPath(string csvPath) => Log.Func($"csvPath: {csvPath}", l =>
        {
            // Handle cases where it's a "file:72"
            if (ValueConverterBase.CouldBeReference(csvPath))
            {
                l.A($"This seems to be a reference: '{csvPath}'");
                csvPath = _serverPaths.FullPathOfReference(csvPath);
                l.A($"Resolved to '{csvPath}'");
                return csvPath;
            }

            l.A("Doesn't seem to be a reference, will use as is");

            // if it's a full path, use that, otherwise do map-path assuming it must be in the app
            // this is for backward compatibility, because old samples used "[App:Path]/something.csv" which returns a relative path
            var result = csvPath.Contains(":") ? csvPath : _serverPaths.FullContentPath(csvPath);
            return result;
        });


        /// <summary>
        /// Delimiter character in the CSV, usually a ',' or ';' but could also be a tab or something. Default is tab.
        /// </summary>
        [Configuration(Fallback = "\t")]
        public string Delimiter
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
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
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// Column in the CSV which contains the ID. 
        /// </summary>
        [Configuration]
        public string IdColumnName
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }


        /// <summary>
        /// The CSV column containing the title of the item - for dropdowns etc. and the EntityTitle property. 
        /// </summary>
        [Configuration]
        public string TitleColumnName
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }


        [PrivateApi]
        public CsvDataSource(MyServices services, IDataFactory dataFactory, IUser user, IServerPaths serverPaths) : base(services, $"{DataSourceConstants.LogPrefix}.Csv")
        {
            ConnectServices(
                _user = user,
                _serverPaths = serverPaths,
                _dataFactory = dataFactory
            );
            ProvideOut(GetList);
        }
        private readonly IUser _user;
        private readonly IServerPaths _serverPaths;
        private readonly IDataFactory _dataFactory;


        private IImmutableList<IEntity> GetList() => Log.Func(l =>
        {
            Configuration.Parse();

            var entityList = new List<IEntity>();

            var csvPath = GetServerPath(FilePath);
            l.A($"CSV path:'{csvPath}', delimiter:'{Delimiter}'");

            if (string.IsNullOrWhiteSpace(csvPath))
                return (Error.Create(title: "No Path Given", message: "There was no path for loading the CSV file."), "error");

            var pathPart = Path.GetDirectoryName(csvPath);
            if (!Directory.Exists(pathPart))
            {
                l.A($"Didn't find path '{pathPart}'");
                return (Error.Create(title: "Path not found",
                        message: _user?.IsSystemAdmin == true
                            ? $"Path for Super User only: '{pathPart}'"
                            : "The path given was not found. For security reasons it's not included in the message. You'll find it in the Insights."), "error");
            }

            if (!File.Exists(csvPath))
                return (Error.Create(title: "CSV File Not Found",
                        message: _user?.IsSystemAdmin == true
                            ? $"Path for Super User only: '{csvPath}'"
                            : "For security reasons the path isn't mentioned here. You'll find it in the Insights."), "error");

            const string commonErrorsIdTitle =
                "A common mistake is to use the wrong delimiter (comma / semi-colon) in which case this may also fail. ";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = Delimiter,
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
                var headers = parser.Record;

                // If we should find the Column...
                if (!string.IsNullOrEmpty(IdColumnName))
                {
                    // on first round, check the headers fields
                    // Try to find - first case-sensitive, then insensitive
                    idColumnIndex = Array.FindIndex(headers, name => name == IdColumnName);
                    if (idColumnIndex == -1)
                        idColumnIndex = Array.FindIndex(headers,
                            name => name.Equals(IdColumnName, StringComparison.InvariantCultureIgnoreCase));
                    if (idColumnIndex == -1)
                        return (Error.Create(title: "ID Column not found",
                            message: $"ID column '{IdColumnName}' specified cannot be found in the file. " +
                                     $"The Headers: '{string.Join(",", headers)}'. " +
                                     $"{commonErrorsIdTitle}"), "error");
                }

                if (string.IsNullOrEmpty(TitleColumnName))
                    titleColName = headers[0];
                else
                {
                    // The following is a little bit complicated, but it checks that the title specified exists
                    titleColName = headers.FirstOrDefault(colName => colName == TitleColumnName)
                                   ?? headers.FirstOrDefault(colName =>
                                       colName.Equals(TitleColumnName, StringComparison.InvariantCultureIgnoreCase));
                    if (titleColName == null)
                        return (Error.Create(title: "Title column not found",
                            message: $"Title column '{TitleColumnName}' cannot be found in the file. " +
                                     $"The Headers: '{string.Join(",", headers)}'. " +
                                     $"{commonErrorsIdTitle}"), "error");
                }

                var csvFactory = _dataFactory.New(options: new DataFactoryOptions(appId: Constants.TransientAppId, typeName: ContentType, titleField: titleColName));

                // Parse data
                while (parser.Read())
                {
                    var fields = parser.Record;

                    int entityId;
                    // No ID column specified, so use the row number
                    if (string.IsNullOrEmpty(IdColumnName))
                        entityId = parser.Row;
                    // check if id can be parsed from the current row
                    else if (!int.TryParse(fields[idColumnIndex], out entityId))
                        return (Error.Create(title: ErrorIdNaN,
                                message:
                                $"Row {parser.Row}: ID field '{headers[idColumnIndex]}' cannot be parsed to int. Value was '{fields[idColumnIndex]}'."),
                            "error");

                    var entityValues = new Dictionary<string, object>();
                    for (var i = 0; i < headers.Length; i++)
                        entityValues.Add(headers[i], (i < fields.Length) ? fields[i] : null);

                    entityList.Add(csvFactory.Create(values: entityValues, id: entityId));
                }
            }

            return (entityList.ToImmutableList(), $"{entityList.Count}");
        });
    }
}