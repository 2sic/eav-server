using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Interfaces
{
    public interface IEntity: IEntityLight, IPublish<IEntity>
    {
        object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks = false);

        string GetBestTitle(string[] dimensions);

        new Dictionary<string, IAttribute> Attributes { get; }

        new IAttribute Title { get; }

        /// <summary>
        /// Gets an Attribute by its StaticName
        /// </summary>
        /// <param name="attributeName">StaticName of the Attribute</param>
        new IAttribute this[string attributeName] { get; }

    }
}
