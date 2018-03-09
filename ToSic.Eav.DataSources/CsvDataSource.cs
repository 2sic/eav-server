using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using System.IO;
using System.Web;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Interfaces;
using ContentTypeBuilder = ToSic.Eav.Data.Builder.ContentTypeBuilder;


namespace ToSic.Eav.DataSources
{
    [VisualQuery(GlobalName = "ToSic.Eav.DataSources.CsvDataSource, ToSic.Eav.DataSources",
        Type = DataSourceType.Source, 
        DynamicOut = false,
        ExpectsDataOfType = "|Config ToSic.Eav.DataSources.CsvDataSource")]
    public class CsvDataSource : ExternalDataDataSource
    {
        private const string FilePathKey = "FilePath";

        public string FilePath
        {
            get => Configuration[FilePathKey];
            set => Configuration[FilePathKey] = value;
        }


        public string ServerFilePath => HttpContext.Current != null ? HttpContext.Current.Server.MapPath(FilePath) : FilePath;


        private const string DelimiterKey = "Delimiter";

        public string Delimiter
        {
            get => Configuration[DelimiterKey];
            set => Configuration[DelimiterKey] = value;
        }


        private const string ContentTypeKey = "ContentType";

        public string ContentType
        {
            get => Configuration[ContentTypeKey];
            set => Configuration[ContentTypeKey] = value;
        }


        private const string IdColumnNameKey = "IdColumnName";

        public string IdColumnName
        {
            get => Configuration[IdColumnNameKey];
            set => Configuration[IdColumnNameKey] = value;
        }


        private const string TitleColumnNameKey = "TitleColumnName";

        public string TitleColumnName
        {
            get => Configuration[TitleColumnNameKey];
            set => Configuration[TitleColumnNameKey] = value;
        }


        public CsvDataSource()
        {
            Provide(GetList);
            //Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));

            ConfigMask(FilePathKey, "[Settings:FilePath]");
            ConfigMask(DelimiterKey, "[Settings:Delimiter||\t]");
            ConfigMask(ContentTypeKey, "[Settings:ContentType||Anonymous]");
            ConfigMask(IdColumnNameKey, "[Settings:IdColumnName]", cacheRelevant: false);
            ConfigMask(TitleColumnNameKey, "[Settings:TitleColumnName]", cacheRelevant: false);
            //CacheRelevantConfigurations = new[] { FilePathKey, DelimiterKey, ContentTypeKey };
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