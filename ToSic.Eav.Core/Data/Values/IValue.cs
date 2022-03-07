using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Value in the EAV system. Values belong to an attribute and can belong to multiple languages. 
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IValue
    {
        /// <summary>
        /// Gets the Language (<see cref="ILanguage"/>) assigned to this Value. Can be one or many. 
        /// </summary>
        IList<ILanguage> Languages { get; set; }

        /// <summary>
        /// The internal contents of the value as a .net object.
        /// Should usually not be used, it's more for internal use.
        /// </summary>
        [PrivateApi]
        object ObjectContents { get; }

        /// <summary>
        /// Returns the inner value in a form that can be serialized, for JSON serialization etc.
        /// </summary>
        object SerializableObject { get; }

        /// <summary>
        /// Older way to serialize the value - used for the XML export/import and save to DB but shouldn't be used elsewhere.
        /// </summary>
        [PrivateApi("not sure why we have two serialization systems, probably will deprecate this some day")]
        string Serialized { get; }


        [PrivateApi("very experimental in 12.05")]
        bool? DynamicUseCache { get; set; }
        [PrivateApi("very experimental in 12.05")]
        object DynamicCache { get; set; }
    }

}
