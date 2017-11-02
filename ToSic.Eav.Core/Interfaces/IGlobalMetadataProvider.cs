using System.Collections.Immutable;

namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// This interface allows objects to provid metadata from "remote" systems
    /// meaning from apps / sources which the original source doesn't know about
    /// </summary>
    public interface IGlobalMetadataProvider
    {
        int GetType(string typeName);

        string GetType(int typeId);

        ImmutableDictionary<int, string> TargetTypes { get; }
    }
}
