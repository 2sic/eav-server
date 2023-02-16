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
        ExpectsDataOfType = "|Config ToSic.Eav.DataSources.CsvDataSource",
        HelpLink = "https://r.2sxc.org/DsCsv")]
    public class CsvDataSource : ExternalData
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
        public CsvDataSource(Dependencies services, IDataBuilder dataBuilder, IUser user, IServerPaths serverPaths) : base(services, $"{DataSourceConstants.LogPrefix}.Csv")
        {
            ConnectServices(
                _user = user,
                _serverPaths = serverPaths,
                _dataBuilder = dataBuilder.Configure(appId: Constants.TransientAppId, typeName: ContentType)
            );
            Provide(GetList);
        }
        private readonly IUser _user;
        private readonly IServerPaths _serverPaths;
        private readonly IDataBuilder _dataBuilder;


        private ImmutableArray<IEntity> GetList() => Log.Func(() =>
        {
            Configuration.Parse();

            var entityList = new List<IEntity>();

            var csvPath = GetServerPath(FilePath);
            Log.A($"CSV path:'{csvPath}', delimiter:'{Delimiter}'");

            if (string.IsNullOrWhiteSpace(csvPath))
                return (SetError("No Path Given", "There was no path for loading the CSV file."),
                    "error");

            var pathPart = Path.GetDirectoryName(csvPath);
            if (!Directory.Exists(pathPart))
            {
                Log.A($"Didn't find path '{pathPart}'");
                return (SetError("Path not found",
                        _user?.IsSystemAdmin == true
                            ? $"Path for Super User only: '{pathPart}'"
                            : "The path given was not found. For security reasons it's not included in the message. You'll find it in the Insights."),
                    "error");
            }

            if (!File.Exists(csvPath))
                return (SetError("CSV File Not Found",
                        _user?.IsSystemAdmin == true
                            ? $"Path for Super User only: '{csvPath}'"
                            : "For security reasons the path isn't mentioned here. You'll find it in the Insights."),
                    "error");

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
                string titleColName = null;


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
                        return (SetError("ID Column not found",
                            $"ID column '{IdColumnName}' specified cannot be found in the file. " +
                            $"The Headers: '{string.Join(",", headers)}'. " +
                            $"{commonErrorsIdTitle}"), "err");
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
                        return (SetError("Title column not found",
                            $"Title column '{TitleColumnName}' cannot be found in the file. " +
                            $"The Headers: '{string.Join(",", headers)}'. " +
                            $"{commonErrorsIdTitle}"), "err");
                }

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
                    {
                        return (SetError(ErrorIdNaN,
                            $"Row {parser.Row}: ID field '{headers[idColumnIndex]}' cannot be parsed to int. Value was '{fields[idColumnIndex]}'."), "err");
                    }

                    var entityValues = new Dictionary<string, object>();
                    for (var i = 0; i < headers.Length; i++)
                        entityValues.Add(headers[i], (i < fields.Length) ? fields[i] : null);

                    entityList.Add(_dataBuilder.Create(values: entityValues, id: entityId, titleField: titleColName));
                }
            }

            return (entityList.ToImmutableArray(), $"{entityList.Count}");
        });
    }
}