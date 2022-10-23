using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This is a Builder-Object which is used to create any kind of data.
    /// Get it using Dependency Injection
    /// </summary>
    [PrivateApi("Hide implementation")]
    public partial class DataBuilder: HasLog<DataBuilder>, IDataBuilder
    {

        #region Constructor / DI

        /// <summary>
        /// Primary constructor for DI.
        /// We recommend that you always call Init afterwards to supply the logger.
        /// </summary>
        public DataBuilder(MultiBuilder builder) : base("Dta.Buildr")
        {
            _builder = builder;
        }
        private readonly MultiBuilder _builder;

        #endregion

        public const int DefaultAppId = 0;
        public const int DefaultEntityId = 0;
        public const string DefaultTypeName = "unspecified";

        /// <inheritdoc />
        public IContentType Type(string typeName) => _builder.ContentType.Transient(typeName);

        /// <inheritdoc />
        [PublicApi]
        public IEntity Entity(
            Dictionary<string, object> values = null,
            string noParamOrder = Parameters.Protector,
            int appId = DefaultAppId,
            int id = DefaultEntityId,
            string titleField = null,
            string typeName = DefaultTypeName,
            IContentType type = null,
            Guid? guid = null,
            DateTime? created = null,
            DateTime? modified = null
            ) 
            => new Entity(appId, id, type ?? Type(typeName), values, titleField, created: created, modified: modified, guid: guid);

        /// <inheritdoc />
        [PublicApi]
        public IEnumerable<IEntity> Entities(IEnumerable<Dictionary<string, object>> itemValues,
            string noParamOrder = Parameters.Protector,
            int appId = 0,
            string titleField = null,
            string typeName = DefaultTypeName,
            IContentType type = null
            )
        {
            type = type ?? Type(typeName);
            return itemValues.Select(values => Entity(values,
                appId: appId,
                titleField: titleField,
                type: type)
            );
        }

        /// <inheritdoc />
        [PrivateApi]
        public IEntity FakeEntity(int appId)
            => Entity(new Dictionary<string, object> { { Attributes.TitleNiceName, "" } },
                appId: appId,
                typeName: "FakeEntity",
                titleField: Attributes.TitleNiceName
            );
    }
}
