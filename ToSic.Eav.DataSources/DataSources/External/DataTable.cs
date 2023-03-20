using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;
using SqlDataTable = System.Data.DataTable;


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
        private readonly IDataFactory _dataFactory;
        // help Link: https://r.2sxc.org/DsDataTable
		#region Configuration-properties

		/// <summary>
		/// Source DataTable
		/// </summary>
        public SqlDataTable Source { get; set; }

        /// <summary>
        /// Name of the ContentType. Defaults to `Data`
        /// </summary>
        /// <remarks>
        /// * in v15 changed default name to `Data`, previously was just empty.
        /// </remarks>
        [Configuration(Fallback = "Data")]
        public string ContentType
		{
			get => Configuration.GetThis();
            set => Configuration.SetThis(value);
		}

		/// <summary>
		/// Name of the Title Attribute of the Source DataTable
		/// </summary>
		[Configuration(Fallback = Attributes.EntityFieldTitle)]
		public string TitleField
		{
			get => Configuration.GetThis(fallback: Attributes.EntityFieldTitle);
            set => Configuration.SetThis(value);
		}

		/// <summary>
		/// Name of the Column used as EntityId
		/// </summary>
		[Configuration(Fallback = Attributes.EntityFieldId)]
		public string EntityIdField
		{
			get => Configuration.GetThis();
            set => Configuration.SetThis(value);
		}

        /// <summary>
        /// Name of the field which would contain a modified timestamp (date/time)
        /// </summary>
        [Configuration]
		public string ModifiedField
		{
			get => Configuration.GetThis();
            set => Configuration.SetThis(value);
		}
        #endregion

        // Important: This constructor must come BEFORE the other constructors
        // because it is the one which the .net Core DI should use!
        /// <summary>
        /// Initializes a new instance of the DataTableDataSource class
        /// </summary>
        [PrivateApi]
        public DataTable(MyServices services, IDataFactory dataFactory) : base(services, $"{DataSourceConstants.LogPrefix}.ExtTbl")
        {
            ConnectServices(
                _dataFactory = dataFactory
            );
            Provide(GetEntities);
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
        public DataTable Setup(SqlDataTable source, string contentType, string entityIdField = null, string titleField = null, string modifiedField = null)
        {
			Source = source;
            // Only set the values if they were explicitly provided
            // Otherwise leave as is, so they could come from MyConfiguration
            if (contentType.HasValue()) ContentType = contentType;
            if (titleField.HasValue()) TitleField = titleField;
            if (entityIdField.HasValue()) EntityIdField = entityIdField;
            if (titleField.HasValue()) TitleField = titleField;
            if (modifiedField.HasValue()) ModifiedField = modifiedField;

            return this;
        }

        private IImmutableList<IEntity> GetEntities() => Log.Func(l =>
        {
            Configuration.Parse();

            l.A($"get type:{ContentType}, id:{EntityIdField}, title:{TitleField}, modified:{ModifiedField}");
            var result = ConvertToEntityDictionary(Source, ContentType, EntityIdField, TitleField, ModifiedField);
            return (result, $"ok: {result.Count}");
        });

        /// <summary>
        /// Convert a DataTable to a Dictionary of EntityModels
        /// </summary>
        private IImmutableList<IEntity> ConvertToEntityDictionary(SqlDataTable source, string contentType, string entityIdField, string titleField, string modifiedField = null
        ) => Log.Func(() =>
        {
            // Validate Columns
            if (!source.Columns.Contains(entityIdField))
                throw new Exception($"DataTable doesn't contain an EntityId Column with Name \"{entityIdField}\"");
            if (!source.Columns.Contains(titleField))
                throw new Exception($"DataTable doesn't contain an EntityTitle Column with Name \"{titleField}\"");

            var tblFactory = _dataFactory.New(appId: Constants.TransientAppId, typeName: contentType, titleField: titleField);
            
            // Populate a new Dictionary with EntityModels
            var result = new List<IEntity>();

            foreach (DataRow row in source.Rows)
            {
                var entityId = global::System.Convert.ToInt32(row[entityIdField]);
                var values = row.Table.Columns.Cast<DataColumn>().Where(c => c.ColumnName != entityIdField)
                    .ToDictionary(c => c.ColumnName, c => row.Field<object>(c.ColumnName));
                values = new Dictionary<string, object>(values,
                    StringComparer.InvariantCultureIgnoreCase); // recast to ensure case-insensitive
                var mod = (string.IsNullOrEmpty(modifiedField) ? null : values[modifiedField] as DateTime?) ?? DateTime.MinValue;

                var entity = tblFactory.Create(values, id: entityId, modified: mod);
                result.Add(entity);
            }

            var final = result.ToImmutableList();
            return (final, $"{final.Count}");
        });
    }
}