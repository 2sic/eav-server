using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
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
			Provide(GetEntities);
		    ConfigMask(TitleFieldKey, EntityTitleDefaultColumnName);
		    ConfigMask(EntityIdFieldKey, EntityIdDefaultColumnName);
		    ConfigMask(ModifiedFieldKey, "");
		    ConfigMask(ContentTypeKey, "[Settings:ContentType]");
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
			TitleField = titleField ?? Constants.EntityFieldTitle;
			EntityIdField = entityIdField ?? EntityIdDefaultColumnName;
			TitleField = titleField ?? EntityTitleDefaultColumnName;
		    ModifiedField = modifiedField ?? "";
		}

		private IEnumerable<IEntity> GetEntities()
		{
			EnsureConfigurationIsLoaded();

		    Log.Add($"get type:{ContentType}, id:{EntityIdField}, title:{TitleField}, modified:{ModifiedField}");
            var result = ConvertToEntityDictionary(Source, ContentType, EntityIdField, TitleField, ModifiedField);
		    return result;
		}

		/// <summary>
		/// Convert a DataTable to a Dictionary of EntityModels
		/// </summary>
		private IEnumerable<IEntity> ConvertToEntityDictionary(DataTable source, string contentType, string entityIdField, string titleField, string modifiedField = null)
		{
			// Validate Columns
			if (!source.Columns.Contains(entityIdField))
				throw new Exception($"DataTable doesn't contain an EntityId Column with Name \"{entityIdField}\"");
			if (!source.Columns.Contains(titleField))
				throw new Exception($"DataTable doesn't contain an EntityTitle Column with Name \"{titleField}\"");

			// Pupulate a new Dictionary with EntityModels
			var result = new List<IEntity>();
			foreach (DataRow row in source.Rows)
			{
				var entityId = Convert.ToInt32(row[entityIdField]);
				var values = row.Table.Columns.Cast<DataColumn>().Where(c => c.ColumnName != entityIdField).ToDictionary(c => c.ColumnName, c => row.Field<object>(c.ColumnName));
                values = new Dictionary<string, object>(values, StringComparer.OrdinalIgnoreCase); // recast to ensure case-insensitive
			    var mod = string.IsNullOrEmpty(modifiedField) ? null : values[modifiedField] as DateTime?;
			    var entity = AsEntity(values, titleField, contentType, entityId, modified: mod, appId: Constants.TransientAppId);
				result.Add(entity);
			}
			return result;
		}
	}
}