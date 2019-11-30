﻿using System.Collections.Specialized;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// Look-Up helper to get something from a standard .net NameValueCollection. <br/>
    /// Read more about this in @Specs.LookUp
    /// </summary>
    [PublicApi]
    public class LookUpInNameValueCollection : LookUpBase
    {
	    readonly NameValueCollection _nameValueCollection;
        public LookUpInNameValueCollection(string name, NameValueCollection list)
        {
            Name = name;
            _nameValueCollection = list;
        }
        
        /// <inheritdoc />
        public override string Get(string key, string format, ref bool notFound) 
            => _nameValueCollection == null 
            ? string.Empty 
            : FormatString(_nameValueCollection[key], format);

        /// <inheritdoc/>
        public override bool Has(string key) => throw new System.NotImplementedException();
    }
}