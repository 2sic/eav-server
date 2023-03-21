using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ToSic.Eav.Conventions;
using ToSic.Eav.LookUp;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// This helps a data source get configured.
    /// It manages all the properties which the data source will want to look up, as well as
    /// the LookUp engine which will perform the token resolution
    /// </summary>
    [PublicApi] 
    public interface IDataSourceConfiguration: IGetAccessors<string>, ISetAccessors<string>, ISetAccessors<object>
    {
        /// <summary>
        /// Get a configuration value for a specific property.
        /// Just use `GetThis()` and the method name (which is the key) is added automatically by the compiler.
        /// </summary>
        /// <param name="key">The configuration key. Do not set this; it's auto-added by the compiler.</param>
        /// <returns></returns>
        /// <remarks>Added in v15.04</remarks>
        string GetThis([CallerMemberName] string key = default);

        /// <summary>
        /// Get a configuration value for a specific property, or the fallback.
        /// Just use `GetThis(5)` and the method name (which is the key) is added automatically by the compiler.
        /// </summary>
        /// <typeparam name="T">The data type of the result. Usually optional, because the fallback has this type so it's auto detected.</typeparam>
        /// <param name="fallback">Fallback value if the configuration is missing or can't be parsed into the expected data format.</param>
        /// <param name="key">The configuration key. Do not set this; it's auto-added by the compiler.</param>
        /// <returns>The configuration value or the fallback.</returns>
        /// <remarks>Added in v15.04</remarks>
        T GetThis<T>(T fallback, [CallerMemberName] string key = default);

        /// <summary>
        /// Set a configuration value for a specific property.
        /// Just use `SetThis(value)` and the method name (which is the key) is added automatically by the compiler.
        /// </summary>
        /// <param name="value">The new value</param>
        /// <param name="key">The configuration key. Do not set this; it's auto-added by the compiler.</param>
        /// <remarks>Added in v15.04</remarks>
        void SetThis<T>(T value, [CallerMemberName] string key = default);


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
        [PrivateApi]
        ILookUpEngine LookUpEngine { get; }
    }
}