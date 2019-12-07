﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Case insensitive dynamic read-object for JSON. <br/>
    /// Used in various cases where you start with JSON and want to
    /// provide the contents to custom code without having to mess with
    /// JS/C# code style differences. <br/>
    /// You will usually do things like `AsDynamic(jsonString).FirstName` etc.
    /// </summary>
    [PublicApi]
    public partial class DynamicJacket: DynamicJacketBase<JObject>
    {
        /// <inheritdoc />
        [PrivateApi]
        internal DynamicJacket(JObject originalData) : base(originalData) { }

        /// <inheritdoc />
        public override bool IsList => false;

        /// <summary>
        /// Enable enumeration. Will return the keys, not the values. <br/>
        /// Use the [key] accessor to get the values as <see cref="DynamicJacket"/> or <see cref="DynamicJacketList"/>
        /// </summary>
        /// <returns>the string names of the keys</returns>
        public override IEnumerator<object> GetEnumerator() => OriginalData.Properties().Select(p => p.Name).GetEnumerator();


        /// <summary>
        /// Access the properties of this object.
        /// </summary>
        /// <remarks>
        /// Note that <strong>this</strong> accessor is case insensitive
        /// </remarks>
        /// <param name="key">the key, case-insensitive</param>
        /// <returns>A value (string, int etc.), <see cref="DynamicJacket"/> or <see cref="DynamicJacketList"/></returns>
        public object this[string key] 
            => FindValueOrNull(key, StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Access the properties of this object.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="caseSensitive">true if case-sensitive, false if not</param>
        /// <returns>A value (string, int etc.), <see cref="DynamicJacket"/> or <see cref="DynamicJacketList"/></returns>
        public object this[string key, bool caseSensitive]
            => FindValueOrNull(key, caseSensitive 
                ? StringComparison.Ordinal
                : StringComparison.InvariantCultureIgnoreCase);


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
            result = FindValueOrNull(binder.Name, StringComparison.InvariantCultureIgnoreCase);
            // always say it was found to prevent runtime errors
            return true;
        }

        private object FindValueOrNull(string name, StringComparison comparison)
        {
            if (OriginalData == null || !OriginalData.HasValues)
                return null;

            var found = OriginalData.Properties()
                .FirstOrDefault(
                    p => string.Equals(p.Name, name, comparison));

            return WrapOrUnwrap(found?.Value);
        }

        #endregion

        /// <inheritdoc />
        public override object this[int index] => (_propertyArray ?? (_propertyArray = OriginalData.Properties().ToArray()))[index];

        private JProperty[] _propertyArray;

    }
}
