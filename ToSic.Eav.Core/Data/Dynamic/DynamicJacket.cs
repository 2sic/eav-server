using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Case insensitive dynamic read-object for JSON. <br/>
    /// Used in various cases where you start with JSON and want to provide the contents to custom code without having to mess with
    /// JS/C# code style differences. 
    /// </summary>
    [PrivateApi("publish later")]
    public partial class DynamicJacket: DynamicObject, IEnumerable<object>
    {
        /// <summary>
        /// The underlying data, in case it's needed for various internal operations
        /// </summary>
        public readonly JToken OriginalData;

        /// <summary>
        /// Primary constructor expecting a Newtonsoft JObject
        /// </summary>
        /// <param name="originalData">the original data we're wrapping</param>
        public DynamicJacket(JObject originalData) => OriginalData = originalData;


        /// <summary>
        /// Enable enumeration. When going through objects (properties) it will return the keys, not the values. <br/>
        /// Use the [key] accessor to get the values as <see cref="DynamicJacket"/> or <see cref="DynamicJacketList"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<object> GetEnumerator() =>
            OriginalData is JObject jObject
                ? jObject.Properties().Select(p => p.Name).GetEnumerator()
                : throw new NotImplementedException();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Access the properties of this object, but only if the underlying object is a real object and not an array.
        /// </summary>
        /// <remarks>
        /// Note that this accessor is case sensitive
        /// </remarks>
        /// <param name="key">the key, case-sensitive</param>
        /// <returns></returns>
        public object this[string key] => OriginalData is JObject jObject ? WrapOrUnwrap(jObject[key]) : null;


        /// <summary>
        /// Performs a case-insensitive value look-up
        /// </summary>
        /// <param name="binder">.net binder object</param>
        /// <param name="result">usually a <see cref="DynamicJacket"/>, <see cref="DynamicJacketList"/> or null</param>
        /// <returns>always returns true, to avoid errors</returns>
        [PrivateApi]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (OriginalData == null || !OriginalData.HasValues)
            {
                result = null;
                return true;
            }

            if (OriginalData is JObject jData)
            {
                var found = jData.Properties()
                    .FirstOrDefault(
                        p => string.Equals(p.Name, binder.Name, StringComparison.InvariantCultureIgnoreCase));

                if (found != null)
                {
                    var original = found.Value;
                    result = WrapOrUnwrap(original);
                    return true;
                }
            }

            // not found
            result = null;
            return true;
        }

    }
}
