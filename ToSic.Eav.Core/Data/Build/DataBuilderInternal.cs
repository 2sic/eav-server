using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Builder;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build
{
    /// <summary>
    /// This is a Builder-Object which is used to create any kind of data.
    /// Get it using Dependency Injection
    /// </summary>
    [PrivateApi("Hide implementation")]
    public class DataBuilderInternal: ServiceBase, IDataBuilderInternal
    {
        private readonly LazySvc<EntityBuilder> _entityBuilder;
        private readonly LazySvc<ContentTypeBuilder> _contentTypeBuilder;

        #region Constructor / DI

        /// <summary>
        /// Primary constructor for DI.
        /// We recommend that you always call Init afterwards to supply the logger.
        /// </summary>
        public DataBuilderInternal(LazySvc<ContentTypeBuilder> contentTypeBuilder, LazySvc<EntityBuilder> entityBuilder) : base("Dta.Buildr")
        {
            ConnectServices(
                _contentTypeBuilder = contentTypeBuilder,
                _entityBuilder = entityBuilder
            );
        }


        #endregion

        public const int DefaultAppId = 0;
        public const int DefaultEntityId = 0;
        public const string DefaultTypeName = "unspecified";

        ///// <inheritdoc />
        //private IContentType Type(string typeName) => _contentTypeBuilder.Value.Transient(typeName);

        ///// <inheritdoc />
        //[PublicApi]
        //public IEntity Entity(
        //    Dictionary<string, object> values = null,
        //    string noParamOrder = Parameters.Protector,
        //    int appId = DefaultAppId,
        //    int id = DefaultEntityId,
        //    string titleField = null,
        //    string typeName = DefaultTypeName,
        //    IContentType type = null,
        //    Guid? guid = null,
        //    DateTime? created = null,
        //    DateTime? modified = null
        //    ) 
        //    => new Entity(appId, id, type ?? Type(typeName), values, titleField, created: created, modified: modified, guid: guid);

        // 2022-02-13 2dm - disabled, shouldn't be API any more
        ///// <inheritdoc />
        //[PublicApi]
        //public IEnumerable<IEntity> Entities(IEnumerable<Dictionary<string, object>> itemValues,
        //    string noParamOrder = Parameters.Protector,
        //    int appId = 0,
        //    string titleField = null,
        //    string typeName = DefaultTypeName,
        //    IContentType type = null
        //    )
        //{
        //    type = type ?? Type(typeName);
        //    return itemValues.Select(values => Entity(values,
        //        appId: appId,
        //        titleField: titleField,
        //        type: type)
        //    );
        //}

        ///// <inheritdoc />
        //[PrivateApi]
        //public IEntity FakeEntity(int appId)
        //    => _entityBuilder.Value.Create(
        //        appId: appId,
        //        values: new Dictionary<string, object> { { Attributes.TitleNiceName, "" } },
        //        contentType: Type("FakeEntity"),
        //        titleField: Attributes.TitleNiceName
        //    );
    }
}
