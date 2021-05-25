using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This is a Builder-Object which is used to create any kind of data.
    /// Get it using Dependency Injection
    /// </summary>
    [PrivateApi]
    public partial class DataBuilder: HasLog<DataBuilder>, IDataBuilder
    {
        #region Constructor / DI

        /// <summary>
        /// Primary constructor for DI.
        /// We recommend that you always call Init afterwards to supply the logger.
        /// </summary>
        public DataBuilder() : base("Dta.Buildr") { }

        #endregion

        public const string DefaultTypeName = "unspecified";

        /// <inheritdoc />
        [PublicApi]
        public IEntity Entity(
            Dictionary<string, object> values = null,
            string noParameterOrder = Constants.RandomProtectionParameter,
            int appId = 0,
            int id = 0,
            string titleField = null,
            string typeName = DefaultTypeName,
            ContentType type = null,
            Guid? guid = null,
            DateTime? created = null,
            DateTime? modified = null
            ) 
            => new Entity(appId, id, type ?? ContentTypeBuilder.Fake(typeName), values, titleField, created: created, modified: modified, guid: guid);

        /// <inheritdoc />
        [PublicApi]
        public IEnumerable<IEntity> Entities(IEnumerable<Dictionary<string, object>> itemValues,
            string noParameterOrder = Constants.RandomProtectionParameter,
            int appId = 0,
            string titleField = null,
            string typeName = DefaultTypeName,
            ContentType type = null
            )
            => itemValues.Select(values => Entity(values,
                appId: appId,
                titleField: titleField,
                typeName: typeName)
            );

        /// <summary>
        /// Create a dummy fake entity. It's just used in scenarios where code must have an entity but the
        /// internals are not relevant. Examples are dummy Metadata or dummy Content-Data.
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [PrivateApi]
        public IEntity FakeEntity(int appId)
            => Entity(new Dictionary<string, object> { { Constants.SysFieldTitle, "" } },
                appId: appId,
                typeName: "FakeEntity",
                titleField: Constants.SysFieldTitle
            );
    }
}
