using System.Collections.Immutable;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// This interface allows objects to provide metadata from "remote" systems
    /// meaning from apps / sources which the original source doesn't know about
    /// </summary>
    [PublicApi]
    public interface IGlobalMetadataProvider
    {
        /// <summary>
        /// Look up the type-number of a metadata target type. These are registered in the DB. 
        /// Use this if you know the type-name, but need the type ID
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns>the id of the target type</returns>
        int GetType(string typeName);

        /// <summary>
        /// Look up the type-name of a metadata target type. These are registered in the DB. 
        /// Use this if you know the type-ID, but need the type name
        /// </summary>
        /// <param name="typeId">the type id</param>
        /// <returns>the name of the target type</returns>
        string GetType(int typeId);

        [PrivateApi]
        ImmutableDictionary<int, string> TargetTypes { get; }
    }
}
