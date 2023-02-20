using System;
using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This is a Builder-Object which is used to create any kind of data.
    /// Get it using Dependency Injection
    /// </summary>
    //[PublicApi]
    [PrivateApi("Made private in v15, as people should use the new one - used to be called IDataBuilder")]
    public interface IDataBuilderInternal
    {
        /// <summary>
        /// Create an Entity using a dictionary of values.
        /// 
        /// Read more about [](xref:NetCode.DataSources.Custom.DataBuilder)
        /// </summary>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="appId">optional app id for this item, defaults to the current app</param>
        /// <param name="id">an optional id for this item, defaults to 0</param>
        /// <param name="values">dictionary of values</param>
        /// <param name="titleField">which field should be access if every something wants to know the title of this item</param>
        /// <param name="typeName">an optional type-name - usually not needed, defaults to "unspecified"; alternatively you can specify the type directly</param>
        /// <param name="type">an optional type object - use this OR the typename to specify a type</param>
        /// <param name="guid">an optional guid for this item, defaults to empty guid</param>
        /// <param name="created">creation timestamp</param>
        /// <param name="modified">modified timestamp</param>
        /// <returns></returns>
        IEntity Entity(
            Dictionary<string, object> values = null,
            string noParamOrder = Parameters.Protector,
            int appId = 0,
            int id = 0,
            string titleField = null,
            string typeName = DataBuilderInternal.DefaultTypeName,
            IContentType type = null,
            Guid? guid = null,
            DateTime? created = null,
            DateTime? modified = null
        );

        // 2022-02-13 2dm - disabled, shouldn't be in API any more - wasn't used internally
        ///// <summary>
        ///// Convert a list of value-dictionaries dictionary into a list of entities
        ///// this assumes that the entities don't have an own id or guid, 
        ///// otherwise you should use the single-item command.
        /////
        ///// Read more about [](xref:NetCode.DataSources.Custom.DataBuilder)
        ///// </summary>
        ///// <param name="itemValues">list of value-dictionaries</param>
        ///// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        ///// <param name="titleField">which field should be access if every something wants to know the title of this item</param>
        ///// <param name="typeName">an optional type-name - usually not needed, defaults to "unspecified"; alternatively you can specify the type directly</param>
        ///// <param name="type">an optional type object - use this OR the typename to specify a type</param>
        ///// <param name="appId">optional app id for this item, defaults to the current app</param>
        ///// <returns></returns>
        //IEnumerable<IEntity> Entities(IEnumerable<Dictionary<string, object>> itemValues,
        //    string noParamOrder = Parameters.Protector,
        //    int appId = 0,
        //    string titleField = null,
        //    string typeName = DataBuilder.DefaultTypeName,
        //    IContentType type = null
        //);

        /// <summary>
        /// Create a dummy fake entity. It's just used in scenarios where code must have an entity but the
        /// internals are not relevant. Examples are dummy Metadata or dummy Content-Data.
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [PrivateApi]
        IEntity FakeEntity(int appId);



        // 2022-02-13 2dm - disabled, shouldn't be in API any more - wasn't used internally
        #region Attributes

        ///// <summary>
        ///// Create a new attribute for adding to an Entity.
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        ///// <param name="typeName">optional type name as string, like "String" or "Entity" - note that type OR typeName must be provided</param>
        ///// <param name="type">optional type code - note that type OR typeName must be provided</param>
        ///// <param name="values">list of values to add to this attribute</param>
        ///// <returns></returns>
        //IAttribute Attribute(string name,
        //    string noParamOrder = Parameters.Protector,
        //    string typeName = null,
        //    ValueTypes type = ValueTypes.Undefined,
        //    IList<IValue> values = null);

        #endregion

        #region Content-Type

        /// <summary>
        /// Create a fake content-type using the specified name. 
        /// </summary>
        /// <param name="typeName">Name to use for this content-type</param>
        /// <returns></returns>
        IContentType Type(string typeName);

        #endregion

    }
}
