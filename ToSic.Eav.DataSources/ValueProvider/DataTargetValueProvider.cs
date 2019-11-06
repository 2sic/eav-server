using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.ValueProviders;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ValueProvider
{
	/// <inheritdoc />
	/// <summary>
	/// Property Accessor to test a Pipeline with Static Values
	/// </summary>
	public class DataTargetValueProvider : BaseValueProvider
	{
	    private readonly IDataTarget _dataTarget;

		/// <summary>
		/// List with static properties and Test-Values
		/// </summary>

		/// <summary>
		/// The class constructor
		/// </summary>
		public DataTargetValueProvider(IDataTarget dataTarget)
		{
		    _dataTarget = dataTarget;
			Name = "In";
		}

        /// <summary>
        /// Will check if any streams in In matches the requested next key-part and will retrieve the first entity in that stream
        /// to deliver the required sub-key (or even sub-sub-key)
        /// </summary>
        /// <param name="property"></param>
        /// <param name="format"></param>
        /// <param name="propertyNotFound"></param>
        /// <returns></returns>
		public override string Get(string property, string format, ref bool propertyNotFound)
		{
            // Check if it has sub-keys to see if it's trying to match a inbound stream
            var subTokens = CheckAndGetSubToken(property);
            // var propertyMatch = SubProperties.Match(property);
		    if (!subTokens.HasSubtoken)
		    {
		        propertyNotFound = true;
		        return string.Empty;
		    }

            // check if this stream exists
		    if (!_dataTarget.In.ContainsKey(subTokens.Source))
		    {
                propertyNotFound = true;
                return string.Empty;
            }

            // check if any entities exist in this specific in-stream
            var entityStream = _dataTarget.In[subTokens.Source];
            if (!entityStream.List.Any())
		    {
                propertyNotFound = true;
                return string.Empty;
            }

            // Create an EntityValueProvider based on the first item, return its Get
		    var first = entityStream.List.First();
		    return new EntityValueProvider(first).Get(subTokens.Rest, format, ref propertyNotFound);

		}

	    public override bool Has(string property)
	    {
	        throw new System.NotImplementedException();
	    }
	}
}
