using System;
using System.Collections.Generic;

namespace ToSic.Eav.LookUp
{
	/// <summary>
	/// Look Up values from a dictionary
	/// </summary>
	public class LookUpInDictionary : LookUpBase
	{
		/// <summary>
		/// List with static properties and Test-Values
		/// </summary>
		public Dictionary<string, string> Properties { get; }

        /// <summary>
        /// Constructor, can optionally take a dictionary to reference with, otherwise creates a new one
        /// </summary>
        /// <param name="name">Name to use</param>
        /// <param name="valueList">value list (dictionary) to reference</param>
        public LookUpInDictionary(string name, Dictionary<string, string> valueList = null)
		{
			Properties = valueList ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Name = name;
		}

        /// <inheritdoc/>
        public override string Get(string key, string format, ref bool notFound)
		{
			try
			{
				return Properties[key];
			}
			catch (KeyNotFoundException)
			{
				notFound = true;
				return null;
			}
		}

        /// <inheritdoc/>
        public override bool Has(string key)
        {
            return Properties.ContainsKey(key);
        }
	}
}
