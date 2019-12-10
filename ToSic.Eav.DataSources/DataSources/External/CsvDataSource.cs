using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using System.IO;
using System.Web;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ContentTypeBuilder = ToSic.Eav.Data.Builder.ContentTypeBuilder;
using IEntity = ToSic.Eav.Data.IEntity;


namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// DataSource for importing/reading CSV files. 
    /// </summary>
    [PublicApi]
    [VisualQuery(GlobalName = "ToSic.Eav.DataSources.CsvDataSource, ToSic.Eav.DataSources",
        Type = DataSourceType.Source, 
        DynamicOut = false,
        ExpectsDataOfType = "|Config ToSic.Eav.DataSources.CsvDataSource")]
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
        public string ServerFilePath => HttpContext.Current != null ? HttpContext.Current.Server.MapPath(FilePath) : FilePath;


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
        public CsvDataSource()
        {
            Provide(GetList);

            ConfigMask(FilePathKey, "[Settings:FilePath]");
            ConfigMask(DelimiterKey, "[Settings:Delimiter||\t]");
            ConfigMask(ContentTypeKey, "[Settings:ContentType||Anonymous]");
            ConfigMask(IdColumnNameKey, "[Settings:IdColumnName]", cacheRelevant: false);
            ConfigMask(TitleColumnNameKey, "[Settings:TitleColumnName]", cacheRelevant: false);
        }


        private IEnumerable<IEntity> GetList()
        {
            EnsureConfigurationIsLoaded();

            var entityList = new List<IEntity>();

            Log.Add($"load csv:{ServerFilePath}, delimit:'{Delimiter}'");
            using (var stream = new StreamReader(ServerFilePath))
            using (var parser = new CsvReader(stream))
            {
                parser.Configuration.Delimiter = Delimiter;
                parser.Configuration.HasHeaderRecord = true;
                parser.Configuration.TrimHeaders = true;
                parser.Configuration.TrimFields = true;

                // Parse data
                while (parser.Read())
                {
                    var fields = parser.CurrentRecord;

                    int entityId;
                    if (string.IsNullOrEmpty(IdColumnName))
                    {   // No ID column specified, so use the row number
                        entityId = parser.Row;
                    }
                    else
                    {
                        var idColumnIndex = Array.FindIndex(parser.FieldHeaders, columnName => columnName == IdColumnName);
                        if(idColumnIndex == -1)
                            // ReSharper disable once NotResolvedInText
                            throw new ArgumentException("ID column specified cannot be found in the file.", "IdColumnName");

                        if (!int.TryParse(fields[idColumnIndex], out entityId))
                            throw new FormatException("Row " + parser.Row + ": ID field '" + fields[idColumnIndex] + "' cannot be parsed.");
                    }

                    string entityTitleName;
                    if (string.IsNullOrEmpty(TitleColumnName))
                    {
                        entityTitleName = parser.FieldHeaders[0];
                    } 
                    else
                    {   // The following is a little bit complicated, but it checks that the title specified exists
                        entityTitleName = parser.FieldHeaders.FirstOrDefault(columnName => columnName == TitleColumnName);
                        if (entityTitleName == null)
                            // ReSharper disable once NotResolvedInText
                            throw new ArgumentException("Title column specified cannot be found in the file.", "TitleColumnName");
                    }
                    
                    var entityValues = new Dictionary<string, object>();
                    for (var i = 0; i < parser.FieldHeaders.Length; i++)
                    {
                        entityValues.Add(parser.FieldHeaders[i], fields[i]);
                    }

                    entityList.Add(new Data.Entity(Constants.TransientAppId, entityId, ContentTypeBuilder.Fake(ContentType), entityValues, entityTitleName));
                }
            }
            Log.Add($"found:{entityList.Count}");
            return entityList;
        }
    }
}