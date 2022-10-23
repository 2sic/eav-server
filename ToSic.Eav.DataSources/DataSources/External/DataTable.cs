using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Lib.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Provide Entities from a System.Data.DataTable. <br/>
	/// This is not meant for VisualQuery, but for code which pre-processes data in a DataTable and then wants to provide it as entities. 
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public class DataTable : ExternalData
	{
        // help Link: https://r.2sxc.org/DsDataTable
		#region Configuration-properties

		/// <inheritdoc/>
		[PrivateApi]
        public override string LogId => "DS.DtaTbl";

        private const string TitleFieldKey = "TitleField";
		private const string EntityIdFieldKey = "EntityIdField";
		private const string ContentTypeKey = "ContentType";
	    private const string ModifiedFieldKey = "ModifiedField";

		/// <summary>
		/// Default Name of the EntityId Column
		/// </summary>
		internal static readonly string EntityIdDefaultColumnName = "EntityId";

	    /// <summary>
	    /// Default Name of the EntityTitle Column
	    /// </summary>
	    internal static readonly string EntityTitleDefaultColumnName = Attributes.EntityFieldTitle; 

		/// <summary>
		/// Source DataTable
		/// </summary>
        public global::System.Data.DataTable Source { get; set; }

		/// <summary>
		/// Name of the ContentType
		/// </summary>
		public string ContentType
		{
			get => Configuration[ContentTypeKey];
		    set => Configuration[ContentTypeKey] = value;
		}

		/// <summary>
		/// Name of the Title Attribute of the Source DataTable
		/// </summary>
		public string TitleField
		{
			get => Configuration[TitleFieldKey];
		    set => Configuration[TitleFieldKey] = value;
		}

		/// <summary>
		/// Name of the Column used as EntityId
		/// </summary>
		public string EntityIdField
		{
			get => Configuration[EntityIdFieldKey];
		    set => Configuration[EntityIdFieldKey] = value;
		}

        /// <summary>
        /// Name of the field which would contain a modified timestamp (date/time)
        /// </summary>
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
        [PrivateApi]
        public DataTable()
		{
            Provide(GetEntities);
		    ConfigMask(TitleFieldKey, EntityTitleDefaultColumnName);
		    ConfigMask(EntityIdFieldKey, EntityIdDefaultColumnName);
		    ConfigMask(ModifiedFieldKey, "");
		    ConfigMask(ContentTypeKey, "[Settings:ContentType]");
        }

        /// <summary>
        /// Initializes a new instance of the DataTableDataSource class with all important parameters.
        /// </summary>
        /// <param name="source">Source object containing the table</param>
        /// <param name="contentType">Type-name to use</param>
        /// <param name="entityIdField">ID column in the table</param>
        /// <param name="titleField">Title column in the table</param>
        /// <param name="modifiedField">modified column in the table</param>
        /// <remarks>
        /// Before 12.09 this was a constructor, but couldn't actually work because it wasn't DI compatible any more.
        /// So we changed it, assuming it wasn't actually used as a constructor before, but only in test code. Marked as private for now
        /// </remarks>
        [PrivateApi]
        public DataTable Setup(global::System.Data.DataTable source, string contentType, string entityIdField = null, string titleField = null, string modifiedField = null)
        {
			Source = source;
			ContentType = contentType;
			TitleField = titleField ?? Attributes.EntityFieldTitle;
			EntityIdField = entityIdField ?? EntityIdDefaultColumnName;
			TitleField = titleField ?? EntityTitleDefaultColumnName;
		    ModifiedField = modifiedField ?? "";

            return this;
        }

		private ImmutableArray<IEntity> GetEntities()
		{
            Configuration.Parse();

            Log.A($"get type:{ContentType}, id:{EntityIdField}, title:{TitleField}, modified:{ModifiedField}");
            var result = ConvertToEntityDictionary(Source, ContentType, EntityIdField, TitleField, ModifiedField);
		    return result;
		}

		/// <summary>
		/// Convert a DataTable to a Dictionary of EntityModels
		/// </summary>
		private ImmutableArray<IEntity> ConvertToEntityDictionary(global::System.Data.DataTable source, string contentType, string entityIdField, string titleField, string modifiedField = null)
		{
			var wrapLog = Log.Fn<ImmutableArray<IEntity>>();

			// Validate Columns
			if (!source.Columns.Contains(entityIdField))
				throw new Exception($"DataTable doesn't contain an EntityId Column with Name \"{entityIdField}\"");
			if (!source.Columns.Contains(titleField))
				throw new Exception($"DataTable doesn't contain an EntityTitle Column with Name \"{titleField}\"");

			// Populate a new Dictionary with EntityModels
			var result = new List<IEntity>();
            var builder = DataBuilder;
			foreach (DataRow row in source.Rows)
			{
				var entityId = global::System.Convert.ToInt32(row[entityIdField]);
				var values = row.Table.Columns.Cast<DataColumn>().Where(c => c.ColumnName != entityIdField).ToDictionary(c => c.ColumnName, c => row.Field<object>(c.ColumnName));
                values = new Dictionary<string, object>(values, StringComparer.InvariantCultureIgnoreCase); // recast to ensure case-insensitive
			    var mod = string.IsNullOrEmpty(modifiedField) ? null : values[modifiedField] as DateTime?;
                var entity = builder.Entity(values,
                    titleField: titleField,
                    typeName: contentType,
                    id: entityId,
                    modified: mod,
                    appId: Constants.TransientAppId);
				result.Add(entity);
			}

            var final = result.ToImmutableArray();
			return wrapLog.Return(final, $"{final.Length}");
		}
	}
}