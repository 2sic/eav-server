using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Provide Entities from a System.Data.DataTable
	/// </summary>
	public sealed class DataTableDataSource : ExternalDataDataSource
	{
        #region Configuration-properties

	    public override string LogId => "DS.DtaTbl";

        private const string TitleFieldKey = "TitleField";
		private const string EntityIdFieldKey = "EntityIdField";
		private const string ContentTypeKey = "ContentType";
	    private const string ModifiedFieldKey = "ModifiedField";

		/// <summary>
		/// Default Name of the EntityId Column
		/// </summary>
		public static readonly string EntityIdDefaultColumnName = "EntityId";

	    /// <summary>
	    /// Default Name of the EntityTitle Column
	    /// </summary>
	    public static readonly string EntityTitleDefaultColumnName = Constants.EntityFieldTitle; 

		/// <summary>
		/// Source DataTable
		/// </summary>
		public DataTable Source { get; set; }

		/// <summary>
		/// Gets or sets the Name of the ContentType
		/// </summary>
		public string ContentType
		{
			get => Configuration[ContentTypeKey];
		    set => Configuration[ContentTypeKey] = value;
		}

		/// <summary>
		/// Gets or sets the Name of the Title Attribute of the Source DataTable
		/// </summary>
		public string TitleField
		{
			get => Configuration[TitleFieldKey];
		    set => Configuration[TitleFieldKey] = value;
		}

		/// <summary>
		/// Gets or sets the Name of the Column used as EntityId
		/// </summary>
		public string EntityIdField
		{
			get => Configuration[EntityIdFieldKey];
		    set => Configuration[EntityIdFieldKey] = value;
		}

	    // private bool _hasModifiedField;
		public string ModifiedField
		{
			get => Configuration[ModifiedFieldKey];
		    set => Configuration[ModifiedFieldKey] = value;
		}
        #endregion

        // Important: This constructor must come BEFORE the other constructors
        // because it is the one which the .net Core DI should use!
        /// <summary>
        /// Initializes a new instance of the DataTableDataSource class
        /// </summary>
        public DataTableDataSource()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetEntities));
			Configuration.Add(TitleFieldKey, EntityTitleDefaultColumnName);
			Configuration.Add(EntityIdFieldKey, EntityIdDefaultColumnName);
			Configuration.Add(ModifiedFieldKey, "");
			Configuration.Add(ContentTypeKey, "[Settings:ContentType]");

            CacheRelevantConfigurations = new[] { ContentTypeKey };
        }

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the DataTableDataSource class
		/// </summary>
		public DataTableDataSource(DataTable source, string contentType, string entityIdField = null, string titleField = null, string modifiedField = null)
			: this()
		{
			Source = source;
			ContentType = contentType;
			TitleField = titleField;
			EntityIdField = entityIdField ?? EntityIdDefaultColumnName;
			TitleField = titleField ?? EntityTitleDefaultColumnName;
		    ModifiedField = modifiedField ?? "";
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();

            var result = ConvertToEntityDictionary(Source, ContentType, EntityIdField, TitleField, ModifiedField);
		    Log.Add($"get type:{ContentType}, id:{EntityIdField}, title:{TitleField}, modified:{ModifiedField}");
		    return result;
		}

		/// <summary>
		/// Convert a DataTable to a Dictionary of EntityModels
		/// </summary>
		private static Dictionary<int, IEntity> ConvertToEntityDictionary(DataTable source, string contentType, string entityIdField, string titleField, string modifiedField = null)
		{
			// Validate Columns
			if (!source.Columns.Contains(entityIdField))
				throw new Exception($"DataTable doesn't contain an EntityId Column with Name \"{entityIdField}\"");
			if (!source.Columns.Contains(titleField))
				throw new Exception($"DataTable doesn't contain an EntityTitle Column with Name \"{titleField}\"");

			// Pupulate a new Dictionary with EntityModels
			var result = new Dictionary<int, IEntity>();
			foreach (DataRow row in source.Rows)
			{
				var entityId = Convert.ToInt32(row[entityIdField]);
				var values = row.Table.Columns.Cast<DataColumn>().Where(c => c.ColumnName != entityIdField).ToDictionary(c => c.ColumnName, c => row.Field<object>(c.ColumnName));
			    var mod = string.IsNullOrEmpty(modifiedField) ? null : values[modifiedField] as DateTime?;
				var entity = new Entity(Constants.TransientAppId, entityId, contentType, values, titleField, mod);
				result.Add(entity.EntityId, entity);
			}
			return result;
		}
	}
}