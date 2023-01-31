using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ToSic.Eav.LookUp;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// This helps a data source get configured.
    /// It manages all the properties which the data source will want to look up, as well as
    /// the LookUp engine which will perform the token resolution
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode] 
    public partial interface IDataSourceConfiguration
    {
        /// <summary>
        /// Quick read / add for values which the DataSource will use. 
        /// </summary>
        /// <param name="key">The property name/key, like CaseInsensitive or similar</param>
        /// <param name="value">A string based value</param>
        string this[string key] { get; set; }

        [PrivateApi("Still WIP v15")]
        string GetThis([CallerMemberName] string cName = default);

        [PrivateApi("Still WIP v15")]
        void SetThis(string value, [CallerMemberName] string cName = default);

        /// <summary>
        /// Tell us if the values have already been parsed or not.
        /// Ideal to check / avoid multiple calls to parse, which would just slow the system down.
        /// </summary>
        bool IsParsed { get; }

        /// <summary>
        /// Parse the values and change them so placeholders in the values are now the resolved value.
        /// This can only be called once - then the placeholder are gone.
        /// In scenarios where multiple parses are required, use the Parse(IDictionary) overload.
        /// </summary>
        void Parse();

        /// <summary>
        /// This will parse a dictionary of values and return the result.
        /// It's used to resolve the values list without actually changing the values on the configuration object,
        /// in scenarios where multiple parses will be required. 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        IDictionary<string, string> Parse(IDictionary<string, string> values);

        /// <summary>
        /// The values (and keys) used in the data source which owns this Configuration
        /// </summary>
        IDictionary<string, string> Values { get; }

        /// <summary>
        /// The internal look up engine which manages value sources and will resolve the tokens
        /// </summary>
        ILookUpEngine LookUpEngine { get; }
    }
}