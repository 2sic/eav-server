using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Lib.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.LookUp
{
	/// <inheritdoc />
	/// <summary>
	/// Look up stuff in a DataSource.
	/// It will take the first <see cref="IEntity"/> in a source and look up properties/attributes in that. <br/>
	/// Normally this is used in Queries, where you want to get a parameter from the <em>In</em> stream.
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public class LookUpInDataTarget : LookUpBase
    {
        /// <summary>
        /// The stream name to read when finding the entity which is the source of this.
        /// </summary>
        public const string InStreamName = "In";

	    private readonly IDataTarget _dataTarget;
        private readonly IZoneCultureResolver _cultureResolver;

        /// <summary>
		/// Constructor expecting the data-target, of which it will use the In-Stream
		/// </summary>
        public LookUpInDataTarget(IDataTarget dataTarget, IZoneCultureResolver cultureResolver)
		{
		    _dataTarget = dataTarget;
            _cultureResolver = cultureResolver;

            Name = InStreamName;
		}

        private string[] Dimensions => _dimensions ?? (_dimensions = _cultureResolver.SafeLanguagePriorityCodes());

        private string[] _dimensions;


        /// <inheritdoc />
        /// <summary>
        /// Will check if any streams in In matches the requested next key-part and will retrieve the first entity in that stream
        /// to deliver the required sub-key (or even sub-sub-key)
        /// </summary>
		public override string Get(string key, string format)
		{
            // Check if it has sub-keys to see if it's trying to match a inbound stream
            var subTokens = CheckAndGetSubToken(key);
		    if (!subTokens.HasSubtoken) return string.Empty;

            // check if this stream exists
		    if (!_dataTarget.In.ContainsKey(subTokens.Source)) return string.Empty;

            // check if any entities exist in this specific in-stream
            var entityStream = _dataTarget.In[subTokens.Source];
            if (!entityStream.List.Any()) return string.Empty;

            // Create an LookUpInEntity based on the first item, return its Get
		    var first = entityStream.List.First();
		    return new LookUpInEntity("no-name", first, Dimensions).Get(subTokens.Rest, format);

		}
    }
}
