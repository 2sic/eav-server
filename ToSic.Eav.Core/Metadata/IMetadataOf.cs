using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Security;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// A provider for metadata for something.
    /// So if an <see cref="IEntity"/> or an App has metadata, this will provide it. 
    /// </summary>
    /// <remarks>
    /// You can either loop through this object (since it's an `IEnumerable`) or ask for values of the metadata,
    /// no matter on what sub-entity the value is stored on.</remarks>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IMetadataOf: IEnumerable<IEntity>, IHasPermissions
    {
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
        /// <param name="typeName">optional type-name, if provided, will only look at metadata of that type; otherwise (or if null) will look at all metadata items and pick first match</param>
        /// <returns>A typed value. </returns>
        TVal GetBestValue<TVal>(string name, string typeName = null);


        /// <summary>
        /// Get the best matching value in the metadata items.
        /// </summary>
        /// <typeparam name="TVal">expected type, like string, int etc.</typeparam>
        /// <param name="name">attribute name we're looking for</param>
        /// <param name="typeNames">list of type-name in the order to check. if one of the values is null, it will then check all items no matter what type</param>
        /// <returns>A typed value. </returns>
        TVal GetBestValue<TVal>(string name, string[] typeNames);
    }
}
