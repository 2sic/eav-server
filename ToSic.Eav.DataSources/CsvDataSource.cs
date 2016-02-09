﻿using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using System.IO;
using System.Web;


namespace ToSic.Eav.DataSources
{
    [PipelineDesigner]
    public class CsvDataSource : ExternalDataDataSource
    {
        private const string FilePathKey = "FilePath";

        public string FilePath
        {
            get { return Configuration[FilePathKey]; }
            set { Configuration[FilePathKey] = value; }
        }


        public string ServerFilePath
        {
            get { return HttpContext.Current != null ? HttpContext.Current.Server.MapPath(FilePath) : FilePath; }
        }


        private const string DelimiterKey = "Delimiter";

        public string Delimiter
        {
            get { return Configuration[DelimiterKey]; }
            set { Configuration[DelimiterKey] = value; }
        }


        private const string ContentTypeKey = "ContentType";

        public string ContentType
        {
            get { return Configuration[ContentTypeKey]; }
            set { Configuration[ContentTypeKey] = value; }
        }


        private const string IdColumnNameKey = "IdColumnName";

        public string IdColumnName
        {
            get { return Configuration[IdColumnNameKey]; }
            set { Configuration[IdColumnNameKey] = value; }               
        }


        private const string TitleColumnNameKey = "TitleColumnName";

        public string TitleColumnName
        {
            get { return Configuration[TitleColumnNameKey]; }
            set { Configuration[TitleColumnNameKey] = value; }
        }


        public CsvDataSource()
        {
            Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));

            Configuration.Add(FilePathKey, "[Settings:FilePath]");
            Configuration.Add(DelimiterKey, "[Settings:Delimiter||\t]");
            Configuration.Add(ContentTypeKey, "[Settings:ContentType||Anonymous]");
            Configuration.Add(IdColumnNameKey, "[Settings:IdColumnName]");
            Configuration.Add(TitleColumnNameKey, "[Settings:TitleColumnName]");
            CacheRelevantConfigurations = new[] { FilePathKey, DelimiterKey, ContentTypeKey };
        }


        private IEnumerable<IEntity> GetList()
        {
            EnsureConfigurationIsLoaded();

            var entityList = new List<IEntity>();

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
                            throw new ArgumentException("Title column specified cannot be found in the file.", "TitleColumnName");
                    }
                    
                    var entityValues = new Dictionary<string, object>();
                    for (var i = 0; i < parser.FieldHeaders.Length; i++)
                    {
                        entityValues.Add(parser.FieldHeaders[i], fields[i]);
                    }

                    entityList.Add(new Data.Entity(entityId, ContentType, entityValues, entityTitleName));
                }
            }
            return entityList;
        }
    }
}