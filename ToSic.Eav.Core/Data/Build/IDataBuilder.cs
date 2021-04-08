using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This is a Builder-Object which is used to create any kind of data.
    /// Get it using Dependency Injection
    /// </summary>
    [PublicApi]
    public interface IDataBuilder
    {
        #region Plumbing / Logging

        DataBuilder Init(ILog parent);

        DataBuilder Init(ILog parent, string logName);

        #endregion

        /// <summary>
        /// Create an Entity using a dictionary of values.
        /// 
        /// Read more about [](xref:NetCode.DataSources.Custom.DataBuilder)
        /// </summary>
        /// <param name="noParameterOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="appId">optional app id for this item, defaults to the current app</param>
        /// <param name="id">an optional id for this item, defaults to 0</param>
        /// <param name="values">dictionary of values</param>
        /// <param name="titleField">which field should be access if every something wants to know the title of this item</param>
        /// <param name="typeName">an optional type-name - usually not needed, defaults to "unspecified"; alternatively you can specify the type directly</param>
        /// <param name="type">an optional type object - use this OR the typename to specify a type</param>
        /// <param name="guid">an optional guid for this item, defaults to empty guid</param>
        /// <param name="modified"></param>
        /// <returns></returns>
        IEntity Entity(
            Dictionary<string, object> values = null,
            string noParameterOrder = Constants.RandomProtectionParameter,
            int appId = 0,
            int id = 0,
            string titleField = null,
            string typeName = DataBuilder.DefaultTypeName,
            ContentType type = null,
            Guid? guid = null,
            DateTime? modified = null
        );

        /// <summary>
        /// Convert a list of value-dictionaries dictionary into a list of entities
        /// this assumes that the entities don't have an own id or guid, 
        /// otherwise you should use the single-item command.
        ///
        /// Read more about [](xref:NetCode.DataSources.Custom.BuildEntity)
        /// </summary>
        /// <param name="itemValues">list of value-dictionaries</param>
        /// <param name="noParameterOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="titleField">which field should be access if every something wants to know the title of this item</param>
        /// <param name="typeName">an optional type-name - usually not needed, defaults to "unspecified"; alternatively you can specify the type directly</param>
        /// <param name="type">an optional type object - use this OR the typename to specify a type</param>
        /// <param name="appId">optional app id for this item, defaults to the current app</param>
        /// <returns></returns>
        IEnumerable<IEntity> Entities(IEnumerable<Dictionary<string, object>> itemValues,
            string noParameterOrder = Constants.RandomProtectionParameter,
            int appId = 0,
            string titleField = null,
            string typeName = DataBuilder.DefaultTypeName,
            ContentType type = null
        );

        /// <summary>
        /// Create a dummy fake entity. It's just used in scenarios where code must have an entity but the
        /// internals are not relevant. Examples are dummy Metadata or dummy Content-Data.
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [PrivateApi]
        IEntity FakeEntity(int appId);


    }
}
