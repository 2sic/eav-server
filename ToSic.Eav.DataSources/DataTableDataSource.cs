using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Provide Entities from a System.Data.DataTable
	/// </summary>
	public class DataTableDataSource : ExternalDataDataSource// BaseDataSource
	{
		#region Configuration-properties

		private const string TitleFieldKey = "TitleField";
		private const string EntityIdFieldKey = "EntityIdField";
		private const string ContentTypeKey = "ContentType";

		/// <summary>
		/// Default Name of the EntityId Column
		/// </summary>
		public static readonly string EntityIdDefaultColumnName = "EntityId";

		/// <summary>
		/// Default Name of the EntityTitle Column
		/// </summary>
		public static readonly string EntityTitleDefaultColumnName = "EntityTitle";

		/// <summary>
		/// Source DataTable
		/// </summary>
		public DataTable Source { get; set; }

		/// <summary>
		/// Gets or sets the Name of the ContentType
		/// </summary>
		public string ContentType
		{
			get { return Configuration[ContentTypeKey]; }
			set { Configuration[ContentTypeKey] = value; }
		}

		/// <summary>
		/// Gets or sets the Name of the Title Attribute of the Source DataTable
		/// </summary>
		public string TitleField
		{
			get { return Configuration[TitleFieldKey]; }
			set { Configuration[TitleFieldKey] = value; }
		}

		/// <summary>
		/// Gets or sets the Name of the Column used as EntityId
		/// </summary>
		public string EntityIdField
		{
			get { return Configuration[EntityIdFieldKey]; }
			set { Configuration[EntityIdFieldKey] = value; }
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the DataTableDataSource class
		/// </summary>
		public DataTableDataSource()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetEntities));
			Configuration.Add(TitleFieldKey, EntityTitleDefaultColumnName);
			Configuration.Add(EntityIdFieldKey, EntityIdDefaultColumnName);
			Configuration.Add(ContentTypeKey, "[Settings:ContentType]");

            CacheRelevantConfigurations = new[] { ContentTypeKey };
        }

		/// <summary>
		/// Initializes a new instance of the DataTableDataSource class
		/// </summary>
		public DataTableDataSource(DataTable source, string contentType, string entityIdField = null, string titleField = null)
			: this()
		{
			Source = source;
			ContentType = contentType;
			TitleField = titleField;
			EntityIdField = entityIdField ?? EntityIdDefaultColumnName;
			TitleField = titleField ?? EntityTitleDefaultColumnName;
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();

			return ConvertToEntityDictionary(Source, ContentType, EntityIdField, TitleField);
		}

		/// <summary>
		/// Convert a DataTable to a Dictionary of EntityModels
		/// </summary>
		private static Dictionary<int, IEntity> ConvertToEntityDictionary(DataTable source, string contentType, string entityIdField, string titleField)
		{
			// Validate Columns
			if (!source.Columns.Contains(entityIdField))
				throw new Exception(string.Format("DataTable doesn't contain an EntityId Column with Name \"{0}\"", entityIdField));
			if (!source.Columns.Contains(titleField))
				throw new Exception(string.Format("DataTable doesn't contain an EntityTitle Column with Name \"{0}\"", titleField));

			// Pupulate a new Dictionary with EntityModels
			var result = new Dictionary<int, IEntity>();
			foreach (DataRow row in source.Rows)
			{
				var entityId = Convert.ToInt32(row[entityIdField]);
				var values = row.Table.Columns.Cast<DataColumn>().Where(c => c.ColumnName != entityIdField).ToDictionary(c => c.ColumnName, c => row.Field<object>(c.ColumnName));
				var entity = new Data.Entity(entityId, contentType, values, titleField);
				result.Add(entity.EntityId, entity);
			}
			return result;
		}
	}
}