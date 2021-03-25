using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Filter entities to show only these belonging to a specific user. 
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Owner Filter",
        UiHint = "Keep only item created by a specified user",
        Icon = "attribution",
        Type = DataSourceType.Security,
        GlobalName = "ToSic.Eav.DataSources.OwnerFilter, ToSic.Eav.DataSources",
        DynamicOut = false,
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.OwnerFilter",
        HelpLink = "https://r.2sxc.org/DsOwnerFilter")]

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

        private ImmutableArray<IEntity> GetList()
        {
            Configuration.Parse();

            Log.Add($"get for identity:{Identity}");
            if (string.IsNullOrWhiteSpace(Identity)) return ImmutableArray<IEntity>.Empty;// new List<IEntity>();

            return In[Constants.DefaultStreamName].Immutable.Where(e => e.Owner == Identity).ToImmutableArray();//.ToList();
        }

	}
}