using System.Collections.Immutable;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// This interface allows objects to lookup metadata-target id / name of the system. 
    /// It basically translates the <see cref="TargetTypes"/> to name and vica versa
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface ITargetTypes
    {
        /// <summary>
        /// Look up the target Id of a metadata target. These are registered somewhere (DB, file-system, etc.)
        /// Use this if you know the type-name, but need the type ID
        /// </summary>
        /// <param name="targetTypeName"></param>
        /// <returns>the id of the target type</returns>
        int GetId(string targetTypeName);

        /// <summary>
        /// Look up the target name of a metadata target. These are registered somewhere (Db, file-system, etc.)
        /// Use this if you know the type-ID, but need the type name
        /// </summary>
        /// <param name="typeId">the type id</param>
        /// <returns>the name of the target type</returns>
        string GetName(int typeId);

        [PrivateApi]
        ImmutableDictionary<int, string> TargetTypes { get; }
    }
}
