using System.Collections.Generic;
using ToSic.Eav.PublicApi;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Interfaces
{
    public interface IMetadataOfItem: IEnumerable<IEntity>, IHasPermissions
    {
        //// 2018-03-09 2dm - this was used when we tried creating code-based content-types, but I believe it's dead code now
        //void Add(string type, Dictionary<string, object> values);

        // 2019-10-27 2dm - I think this is a leftover of old times, I believe it's not needed any more
        void Add(IEntity additionalItem);
        void Use(List<IEntity> items);

        /// <summary>
        /// Get the best matching value in the metadata items.
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="name">attribute name we're looking for</param>
        /// <param name="type">optional type-name, if provided, will only look at metadata of that type; otherwise (or if null) will look at all metadata items and pick first match</param>
        /// <returns></returns>
        TVal GetBestValue<TVal>(string name, string type = null);


        /// <summary>
        /// Get the best matching value in the metadata items.
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="name">attribute name we're looking for</param>
        /// <param name="types">list of type-name in the order to check. if one of the values is null, it will then check all items no matter what type</param>
        /// <returns></returns>
        TVal GetBestValue<TVal>(string name, string[] types);
    }
}
