using System;
using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// Look Up values from a .net dictionary. Case-Insensitive. <br/>
    /// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
    /// </summary>
	[PublicApi_Stable_ForUseInYourCode]
    public class LookUpInDictionary : LookUpBase
	{
		/// <summary>
		/// List with static properties and Test-Values
		/// </summary>
		public IDictionary<string, string> Properties { get; }

        /// <summary>
        /// Constructor, can optionally take a dictionary to reference with, otherwise creates a new one
        /// </summary>
        /// <param name="name">Name to use</param>
        /// <param name="valueList">value list (dictionary) to reference</param>
        public LookUpInDictionary(string name, IDictionary<string, string> valueList = null)
        {
            // either take existing dic or create new, but always make sure it's case insensitive
            Properties = valueList != null
                ? new Dictionary<string, string>(valueList, StringComparer.InvariantCultureIgnoreCase)
                : new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            Name = name;
		}

        /// <inheritdoc/>
        public override string Get(string key, string format)
		{
            // first try a safe check
            if (!Properties.ContainsKey(key)) return string.Empty;

            // then attempt the try/catch way
			try
			{
				return Properties[key] ?? string.Empty;
			}
			catch (KeyNotFoundException)
			{
				return string.Empty;
			}
		}
    }
}
