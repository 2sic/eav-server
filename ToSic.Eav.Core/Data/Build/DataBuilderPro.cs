using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Raw;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data
{
    [PrivateApi("Still experimental")]
    public class DataBuilderPro : ServiceBase, IDataBuilderPro
    {
        /// <inheritdoc />
        public int AppId { get; private set; }
        /// <inheritdoc />
        public string TitleField { get; private set; }
        /// <inheritdoc />
        public int IdCounter { get; private set; }
        /// <inheritdoc />
        public IContentType ContentType { get; private set; }

        public const int DefaultIdSeed = 1;

        #region Constructor / DI

        private readonly IDataBuilder _parentBuilder;

        /// <summary>
        /// Constructor for DI
        /// </summary>
        /// <param name="parentBuilder"></param>
        public DataBuilderPro(IDataBuilder parentBuilder): base("Ds.DatBld")
        {
            _parentBuilder = parentBuilder;
        }

        #endregion

        //public DataBuilderQuickWIP(IDataBuilder parentBuilder,
        //    string noParamOrder = Parameters.Protector,
        //    int appId = DataBuilder.DefaultAppId, 
        //    string typeName = default,
        //    string titleField = default,
        //    int idSeed = DefaultIdSeed)
        //{
        //    Parameters.ProtectAgainstMissingParameterNames(noParamOrder);
        //    _parentBuilder = parentBuilder;
        //    AppId = appId;
        //    TitleField = titleField;
        //    IdCounter = idSeed;
        //    ContentType = parentBuilder.Type(typeName ?? DataBuilder.DefaultTypeName);
        //}

        /// <inheritdoc />
        public IDataBuilderPro Configure(
            string noParamOrder = Parameters.Protector,
            int appId = DataBuilder.DefaultAppId,
            string typeName = default,
            string titleField = default,
            int idSeed = DefaultIdSeed
        )
        {
            Parameters.ProtectAgainstMissingParameterNames(noParamOrder);
            AppId = appId;
            TitleField = titleField;
            IdCounter = idSeed;
            ContentType = _parentBuilder.Type(typeName ?? DataBuilder.DefaultTypeName);
            return this;
        }

        /// <summary>
        /// For objects which delegate the IRawEntity to a property.
        /// </summary>
        /// <param name="withSource"></param>
        /// <returns></returns>
        public IEntity Create(IHasRawEntitySource withSource) => Create(withSource.Source);

        /// <summary>
        /// For objects which themselves are IRawEntity
        /// </summary>
        /// <param name="rawEntity"></param>
        /// <param name="nullId">when 0 is valid Id for some DataSources, provide Eav.Constants.NullId instead</param>
        /// <returns></returns>
        public IEntity Create(IRawEntity rawEntity, int nullId = 0) => Create(
            rawEntity.RawProperties,
            id: rawEntity.Id == nullId ? null : rawEntity.Id as int?,
            guid: rawEntity.Guid,
            created: rawEntity.Created,
            modified: rawEntity.Modified
        );

        /// <summary>
        /// Use when 0, -1, -2, etc... is valid Id for DataSource
        /// </summary>
        /// <param name="rawEntity"></param>
        /// <returns></returns>
        public IEntity CreateWithEavNullId(IRawEntity rawEntity) => Create(rawEntity, Constants.NullId);

        public IEntity Create(Dictionary<string, object> values, int? id = default, Guid? guid = default, DateTime? created = default, DateTime? modified = default)
        {
            var ent = _parentBuilder.Entity(values,
                appId: AppId,
                id: id ?? IdCounter++,
                type: ContentType,
                titleField: TitleField,
                guid: guid ?? Guid.Empty,
                created: created,
                modified: modified
            );
            return ent;
        }
    }
}
