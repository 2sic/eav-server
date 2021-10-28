using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CsvHelper;
using System.IO;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.Run;
using ContentTypeBuilder = ToSic.Eav.Data.Builder.ContentTypeBuilder;
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
        Icon = "description",
        Type = DataSourceType.Source, 
        GlobalName = "ToSic.Eav.DataSources.CsvDataSource, ToSic.Eav.DataSources",
        DynamicOut = false,
        ExpectsDataOfType = "|Config ToSic.Eav.DataSources.CsvDataSource",
        HelpLink = "https://r.2sxc.org/DsCsv")]
    public class CsvDataSource : ExternalData
    {
        /// <inheritdoc/>
        [PrivateApi]
        public override string LogId => "DS.CSV";

        private const string FilePathKey = "FilePath";

        /// <summary>
        /// Path to the CSV file, relative to the website root
        /// </summary>
        public string FilePath
        {
            get => Configuration[FilePathKey];
            set => Configuration[FilePathKey] = value;
        }

        /// <summary>
        /// Full path to the CSV file. 
        /// </summary>
        public string GetServerPath(string csvPath)
        {
            var wrapLog = Log.Call<string>($"csvPath: {csvPath}");

            // Handle cases where it's a "file:72"
            if (ValueConverterBase.CouldBeReference(csvPath))
            {
                Log.Add($"This seems to be a reference: '{csvPath}'");
                csvPath = _serverPaths.FullPathOfReference(csvPath);
                Log.Add($"Resolved to '{csvPath}'");
                return wrapLog(csvPath, csvPath);
            }

            Log.Add("Doesn't seem to be a reference, will use as is");

            // if it's a full path, use that, otherwise do map-path assuming it must be in the app
            // this is for backward compatibility, because old samples used "[App:Path]/something.csv" which returns a relative path
            var result = csvPath.Contains(":") ? csvPath : _serverPaths.FullContentPath(csvPath);
            return wrapLog(result, result);
        }


        /// <summary>
        /// Delimiter character in the CSV, usually a ',' or ';' but could also be a tab or something. Default is tab.
        /// </summary>
        public string Delimiter
        {
            get => Configuration[DelimiterKey];
            set => Configuration[DelimiterKey] = value;
        }
        private const string DelimiterKey = "Delimiter";


        /// <summary>
        /// Name of the content type which the imported entities have. This is fake, but may be necessary for later filtering of the types. Defaults to "Anonymous"
        /// </summary>
        public string ContentType
        {
            get => Configuration[ContentTypeKey];
            set => Configuration[ContentTypeKey] = value;
        }
        private const string ContentTypeKey = "ContentType";


        /// <summary>
        /// Column in the CSV which contains the ID. 
        /// </summary>
        public string IdColumnName
        {
            get => Configuration[IdColumnNameKey];
            set => Configuration[IdColumnNameKey] = value;
        }
        private const string IdColumnNameKey = "IdColumnName";


        /// <summary>
        /// The CSV column containing the title of the item - for dropdowns etc. and the EntityTitle property. 
        /// </summary>
        public string TitleColumnName
        {
            get => Configuration[TitleColumnNameKey];
            set => Configuration[TitleColumnNameKey] = value;
        }
        private const string TitleColumnNameKey = "TitleColumnName";


        [PrivateApi]
        public CsvDataSource(IUser user, IServerPaths serverPaths)
        {
            _user = user;
            _serverPaths = serverPaths;
            Provide(GetList);

            ConfigMask(FilePathKey, "[Settings:FilePath]");
            ConfigMask(DelimiterKey, "[Settings:Delimiter||\t]");
            ConfigMask(ContentTypeKey, "[Settings:ContentType||Anonymous]");
            ConfigMask(IdColumnNameKey, "[Settings:IdColumnName]", cacheRelevant: false);
            ConfigMask(TitleColumnNameKey, "[Settings:TitleColumnName]", cacheRelevant: false);
        }
        private readonly IUser _user;
        private readonly IServerPaths _serverPaths;


        private ImmutableArray<IEntity> GetList()
        {
            var wrapLog = Log.Call<ImmutableArray<IEntity>>();
            Configuration.Parse();

            var entityList = new List<IEntity>();

            var csvPath = GetServerPath(FilePath);
            Log.Add($"CSV path:'{csvPath}', delimiter:'{Delimiter}'");

            if (string.IsNullOrWhiteSpace(csvPath))
                return wrapLog("error", SetError("No Path Given", "There was no path for loading the CSV file."));

            var pathPart = Path.GetDirectoryName(csvPath);
            if (!Directory.Exists(pathPart))
            {
                Log.Add($"Didn't find path '{pathPart}'");
                return wrapLog("error", SetError("Path not found",
                    _user?.IsSuperUser == true
                        ? $"Path for Super User only: '{pathPart}'"
                        : "The path given was not found. For security reasons it's not included in the message. You'll find it in the Insights."));
            }
            
            if(!File.Exists(csvPath))
                return wrapLog("error",
                    SetError("CSV File Not Found",
                        _user?.IsSuperUser == true
                            ? $"Path for Super User only: '{csvPath}'"
                            : "For security reasons the path isn't mentioned here. You'll find it in the Insights."));

            const string commonErrorsIdTitle =
                "A common mistake is to use the wrong delimiter (comma / semi-colon) in which case this may also fail. ";

            var firstRun = true;
            using (var stream = new StreamReader(csvPath))
            using (var parser = new CsvReader(stream))
            {
                parser.Configuration.Delimiter = Delimiter;
                parser.Configuration.HasHeaderRecord = true;
                parser.Configuration.TrimHeaders = true;
                parser.Configuration.TrimFields = true;

                const int idColumnNotDetermined = -999;
                var idColumnIndex = idColumnNotDetermined;
                string titleColName = null;
                // Parse data
                while (parser.Read())
                {
                    var fields = parser.CurrentRecord;

                    // Check header - must happen after the first read, but we don't want to repeat this
                    if (firstRun)
                    {
                        // If we should find the Column...
                        if (!string.IsNullOrEmpty(IdColumnName))
                        {
                            // on first round, check the headers fields
                            // Try to find - first case-sensitive, then insensitive
                            idColumnIndex = Array.FindIndex(parser.FieldHeaders, name => name == IdColumnName);
                            if (idColumnIndex == -1) 
                                idColumnIndex = Array.FindIndex(parser.FieldHeaders, name => name.Equals(IdColumnName, StringComparison.InvariantCultureIgnoreCase));
                            if (idColumnIndex == -1)
                                return SetError("ID Column not found",
                                    $"ID column '{IdColumnName}' specified cannot be found in the file. " +
                                    $"The Headers: '{string.Join(",", parser.FieldHeaders)}'. " +
                                    $"{commonErrorsIdTitle}");
                        }

                        if (string.IsNullOrEmpty(TitleColumnName))
                            titleColName = parser.FieldHeaders[0];
                        else
                        {
                            // The following is a little bit complicated, but it checks that the title specified exists
                            titleColName = parser.FieldHeaders.FirstOrDefault(colName => colName == TitleColumnName)
                                           ?? parser.FieldHeaders.FirstOrDefault(colName => colName.Equals(TitleColumnName, StringComparison.InvariantCultureIgnoreCase));
                            if (titleColName == null)
                                return SetError("Title column not found",
                                    $"Title column '{TitleColumnName}' cannot be found in the file. " +
                                    $"The Headers: '{string.Join(",", parser.FieldHeaders)}'. " +
                                    $"{commonErrorsIdTitle}");
                        }
                        firstRun = false;
                    }

                    int entityId;
                    // No ID column specified, so use the row number
                    if (string.IsNullOrEmpty(IdColumnName))
                        entityId = parser.Row; 
                    // check if id can be parsed from the current row
                    else if (!int.TryParse(fields[idColumnIndex], out entityId))
                        return SetError("ID is not a number",
                            $"Row {parser.Row}: ID field '{fields[idColumnIndex]}' cannot be parsed to int. Value was '{fields[idColumnIndex]}'.");


                    var entityValues = new Dictionary<string, object>();
                    for (var i = 0; i < parser.FieldHeaders.Length; i++)
                        entityValues.Add(parser.FieldHeaders[i], fields[i]);

                    entityList.Add(new Entity(Constants.TransientAppId, entityId, ContentTypeBuilder.Fake(ContentType), entityValues, titleColName));
                }
            }
            return wrapLog($"{entityList.Count}", entityList.ToImmutableArray());
        }
    }
}