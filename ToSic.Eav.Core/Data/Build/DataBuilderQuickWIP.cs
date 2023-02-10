using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Raw;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    [PrivateApi("Still experimental")]
    public class DataBuilderQuickWIP
    {
        private readonly IDataBuilder _parentBuilder;
        private readonly int _appId;
        private readonly string _titleField;
        private int _entityId;
        private readonly IContentType _contentType;

        public const int DefaultIdSeed = 1;

        public DataBuilderQuickWIP(IDataBuilder parentBuilder,
            string noParamOrder = Parameters.Protector,
            int appId = DataBuilder.DefaultAppId, 
            string typeName = null,
            string titleField = null,
            int idSeed = DefaultIdSeed)
        {
            Parameters.ProtectAgainstMissingParameterNames(noParamOrder);
            _parentBuilder = parentBuilder;
            _appId = appId;
            _titleField = titleField;
            _entityId = idSeed;
            _contentType = parentBuilder.Type(typeName ?? DataBuilder.DefaultTypeName);
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
                appId: _appId,
                id: id ?? _entityId++,
                type: _contentType,
                titleField: _titleField,
                guid: guid ?? Guid.Empty,
                created: created,
                modified: modified
            );
            return ent;
        }
    }
}
