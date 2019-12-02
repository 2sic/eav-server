using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Filter entities to show Drafts or only Published Entities
	/// </summary>
    [PublicApi]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.OwnerFilter, ToSic.Eav.DataSources",
        Type = DataSourceType.Security,
        DynamicOut = false,
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.OwnerFilter",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-OwnerFilter")]

    public class OwnerFilter : DataSourceBase
	{
        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.OwnrF";

        private const string IdentityCode = "IdentityCode";

        /// <summary>
        /// The identity of the user to filter by. Uses the Identity-token convention like dnn:1 is the user #1 in the DNN DB
        /// </summary>
        public string Identity
		{
			get => Configuration[IdentityCode];
            set => Configuration[IdentityCode] = value;
        }
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new PublishingFilter
		/// </summary>
		[PrivateApi]
		public OwnerFilter()
		{
            Provide(GetList);
		    ConfigMask(IdentityCode, "[Settings:" + IdentityCode + "]"); 
        }

        private IEnumerable<IEntity> GetList()
        {
            EnsureConfigurationIsLoaded();

            Log.Add($"get for identity:{Identity}");
            if (string.IsNullOrWhiteSpace(Identity)) return new List<IEntity>();

            return In["Default"].List.Where(e => e.Owner == Identity);
        }

	}
}