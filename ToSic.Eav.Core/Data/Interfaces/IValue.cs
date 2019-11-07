using System.Collections.Generic;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Value
    /// </summary>
    public interface IValue
    {
        /// <summary>
        /// Gets the Languages assigned to this Value
        /// </summary>
        IList<ILanguage> Languages { get; set; }

        object ObjectContents { get; }

        // 2017-06-09 2dm removed, seems unused
        //string Serialized {get;}

        object SerializableObject { get; }

        string Serialized { get; }

        IValue Copy(string type);
    }

}
