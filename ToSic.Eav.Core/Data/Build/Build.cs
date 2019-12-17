using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Build various kinds of Data objects from other things.
    /// This is a simple static accessor for all kinds of builds - the real code is elsewhere.
    /// </summary>
    public static class Build
    {
        public const string DefaultTypeName = "unspecified";

        /// <summary>
        /// Convert a dictionary of values into an entity
        /// </summary>
        /// <param name="noParameterOrder"></param>
        /// <param name="appId">optional app id for this item, defaults to the current app</param>
        /// <param name="id">an optional id for this item, defaults to 0</param>
        /// <param name="values">dictionary of values</param>
        /// <param name="titleField">which field should be access if every something wants to know the title of this item</param>
        /// <param name="typeName">an optional type-name - usually not needed, defaults to "unspecified"</param>
        /// <param name="guid">an optional guid for this item, defaults to empty guid</param>
        /// <param name="modified"></param>
        /// <returns></returns>
        [PrivateApi]
        public static IEntity Entity(
            Dictionary<string, object> values = null,
            string noParameterOrder = Eav.Constants.RandomProtectionParameter,
            int appId = 0,
            int id = 0,
            string titleField = null,
            string typeName = DefaultTypeName,
            Guid? guid = null,
            DateTime? modified = null)
        {
            return new Entity(appId, id, ContentTypeBuilder.Fake(typeName), values, titleField, modified,
                entityGuid: guid);
        }

        /// <summary>
        /// Convert a list of value-dictionaries dictionary into a list of entities
        /// this assumes that the entities don't have an own id or guid, 
        /// otherwise you should use the single-item overload
        /// </summary>
        /// <param name="itemValues">list of value-dictionaries</param>
        /// <param name="titleField">which field should be access if every something wants to know the title of this item</param>
        /// <param name="typeName">an optional type-name - usually not needed, defaults to "unspecified"</param>
        /// <param name="appId">optional app id for this item, defaults to the current app</param>
        /// <returns></returns>
        [PrivateApi]
        public static IEnumerable<IEntity> Entity(int appId, IEnumerable<Dictionary<string, object>> itemValues,
            string titleField = null,
            string typeName = DefaultTypeName)
            => itemValues.Select(values => Entity(values,
                appId: appId,
                titleField: titleField,
                typeName: typeName)
            );

    }
}
