using System;
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
    [PrivateApi("don't publish yet, not sure if this is the right name/namespaces")]
    public partial class DynamicJacket: DynamicJacketBase<JObject>
    {
        /// <inheritdoc />
        [PrivateApi]
        internal DynamicJacket(JObject originalData) : base(originalData) { }

        /// <summary>
        /// Enable enumeration. When going through objects (properties) it will return the keys, not the values. <br/>
        /// Use the [key] accessor to get the values as <see cref="DynamicJacket"/> or <see cref="DynamicJacketList"/>
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<object> GetEnumerator() => OriginalData.Properties().Select(p => p.Name).GetEnumerator();


        /// <summary>
        /// Access the properties of this object, but only if the underlying object is a real object and not an array.
        /// </summary>
        /// <remarks>
        /// Note that this accessor is case sensitive
        /// </remarks>
        /// <param name="key">the key, case-sensitive</param>
        /// <returns>A value (string, int etc.), <see cref="DynamicJacket"/> or <see cref="DynamicJacketList"/></returns>
        public object this[string key] => WrapOrUnwrap(OriginalData[key]);


        #region Private TryGetMember

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

            var found = OriginalData.Properties()
                .FirstOrDefault(
                    p => string.Equals(p.Name, binder.Name, StringComparison.InvariantCultureIgnoreCase));

            if (found != null)
            {
                var original = found.Value;
                result = WrapOrUnwrap(original);
                return true;
            }

            // not found
            result = null;
            return true;
        }
        #endregion

        /// <inheritdoc />
        public override object this[int index] => (_propertyArray ?? (_propertyArray = OriginalData.Properties().ToArray()))[index];

        private JProperty[] _propertyArray;

    }
}
