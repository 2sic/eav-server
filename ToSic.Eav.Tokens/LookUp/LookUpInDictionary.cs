using System;
using System.Collections.Generic;

namespace ToSic.Eav.LookUp
{
	/// <inheritdoc />
	/// <summary>
	/// Property Accessor to test a Pipeline with Static Values
	/// </summary>
	public class LookUpInDictionary : LookUpBase
	{
		/// <summary>
		/// List with static properties and Test-Values
		/// </summary>
		public Dictionary<string, string> Properties { get; }

		/// <summary>
		/// The class constructor, can optionally take a dictionary to reference with, otherwise creates a new one
		/// </summary>
		public LookUpInDictionary(string name, Dictionary<string, string> valueList = null)
		{
			Properties = valueList ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Name = name;
		}

		public override string Get(string key, string format, ref bool propertyNotFound)
		{
			try
			{
				return Properties[key];
			}
			catch (KeyNotFoundException)
			{
				propertyNotFound = true;
				return null;
			}
		}

        public override bool Has(string key)
        {
            return Properties.ContainsKey(key);
        }
	}
}
