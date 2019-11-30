using System.Linq;
using ToSic.Eav.DataSources;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.LookUp
{
	/// <inheritdoc />
	/// <summary>
	/// Property Accessor to test a Pipeline with Static Values
	/// </summary>
	public class LookUpInDataTarget : LookUpBase
	{
	    private readonly IDataTarget _dataTarget;

		/// <summary>
		/// List with static properties and Test-Values
		/// </summary>

		/// <summary>
		/// The class constructor
		/// </summary>
		public LookUpInDataTarget(IDataTarget dataTarget)
		{
		    _dataTarget = dataTarget;
			Name = "In";
		}

        /// <summary>
        /// Will check if any streams in In matches the requested next key-part and will retrieve the first entity in that stream
        /// to deliver the required sub-key (or even sub-sub-key)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="format"></param>
        /// <param name="notFound"></param>
        /// <returns></returns>
		public override string Get(string key, string format, ref bool notFound)
		{
            // Check if it has sub-keys to see if it's trying to match a inbound stream
            var subTokens = CheckAndGetSubToken(key);
            // var propertyMatch = SubProperties.Match(key);
		    if (!subTokens.HasSubtoken)
		    {
		        notFound = true;
		        return string.Empty;
		    }

            // check if this stream exists
		    if (!_dataTarget.In.ContainsKey(subTokens.Source))
		    {
                notFound = true;
                return string.Empty;
            }

            // check if any entities exist in this specific in-stream
            var entityStream = _dataTarget.In[subTokens.Source];
            if (!entityStream.List.Any())
		    {
                notFound = true;
                return string.Empty;
            }

            // Create an LookUpInEntity based on the first item, return its Get
		    var first = entityStream.List.First();
		    return new LookUpInEntity(first).Get(subTokens.Rest, format, ref notFound);

		}

	    public override bool Has(string key)
	    {
	        throw new System.NotImplementedException();
	    }
	}
}
