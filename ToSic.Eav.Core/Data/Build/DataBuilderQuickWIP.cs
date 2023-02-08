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
        /// <returns></returns>
        public IEntity Create(IRawEntity rawEntity) => Create(
            rawEntity.RawProperties,
            id: rawEntity.Id == int.MinValue/* 0*/ ? null : rawEntity.Id as int?, // 0 is valid Id for some DataSources, so using int.MinValue instead
            guid: rawEntity.Guid,
            created: rawEntity.Created,
            modified: rawEntity.Modified
        );

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
