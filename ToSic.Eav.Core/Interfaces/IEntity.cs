using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IEntity: IEntityLight, IPublish<IEntity>
    {
        object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks = false);

        string GetBestTitle(string[] dimensions);

        Dictionary<string, IAttribute> Attributes { get; }

        new IAttribute Title { get; }

        /// <summary>
        /// Gets an Attribute by its StaticName
        /// </summary>
        /// <param name="attributeName">StaticName of the Attribute</param>
        new IAttribute this[string attributeName] { get; }

        int Version { get; }
    }
}
