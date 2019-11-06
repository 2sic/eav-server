using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// A provider for metadata for something.
    /// So if an <see cref="IEntity"/> or an <see cref="ToSic.Eav.Apps.Interfaces.IApp"/> has metadata, this will provide it. 
    /// </summary>
    [PublicApi]
    public interface IMetadataOfItem: IEnumerable<IEntity>, IHasPermissions
    {
        // 2019-10-27 2dm - I think this is a leftover of old times, I believe it's not needed any more
        //void Add(IEntity additionalItem);

        /// <summary>
        /// Internal API to override metadata providing, for example when creating new entities before saving.
        /// </summary>
        /// <param name="items"></param>
        [PrivateApi]
        void Use(List<IEntity> items);

        /// <summary>
        /// Get the best matching value in ALL the metadata items.
        /// </summary>
        /// <typeparam name="TVal">expected type, like string, int etc.</typeparam>
        /// <param name="name">attribute name we're looking for</param>
        /// <param name="type">optional type-name, if provided, will only look at metadata of that type; otherwise (or if null) will look at all metadata items and pick first match</param>
        /// <returns>A typed value. </returns>
        TVal GetBestValue<TVal>(string name, string type = null);


        /// <summary>
        /// Get the best matching value in the metadata items.
        /// </summary>
        /// <typeparam name="TVal">expected type, like string, int etc.</typeparam>
        /// <param name="name">attribute name we're looking for</param>
        /// <param name="types">list of type-name in the order to check. if one of the values is null, it will then check all items no matter what type</param>
        /// <returns>A typed value. </returns>
        TVal GetBestValue<TVal>(string name, string[] types);
    }
}
