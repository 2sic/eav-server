using System;
using System.Collections.Generic;
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

        public IEntity Create(Dictionary<string, object> values)
        {
            var ent = _parentBuilder.Entity(values,
                appId: _appId,
                id: _entityId++,
                type: _contentType,
                titleField: _titleField,
                guid: Guid.Empty
            );
            return ent;
        }
    }
}
