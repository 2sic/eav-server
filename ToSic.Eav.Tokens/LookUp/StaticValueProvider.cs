using System;
using System.Collections.Generic;

namespace ToSic.Eav.ValueProviders
{
	/// <inheritdoc />
	/// <summary>
	/// Property Accessor to test a Pipeline with Static Values
	/// </summary>
	public class StaticValueProvider : BaseValueProvider
	{
		/// <summary>
		/// List with static properties and Test-Values
		/// </summary>
		public Dictionary<string, string> Properties { get; }

		/// <summary>
		/// The class constructor, can optionally take a dictionary to reference with, otherwise creates a new one
		/// </summary>
		public StaticValueProvider(string name, Dictionary<string, string> valueList = null)
		{
			Properties = valueList ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Name = name;
		}

		public override string Get(string property, string format, ref bool propertyNotFound)
		{
			try
			{
				return Properties[property];
			}
			catch (KeyNotFoundException)
			{
				propertyNotFound = true;
				return null;
			}
		}

        public override bool Has(string property)
        {
            return Properties.ContainsKey(property);
        }
	}
}
