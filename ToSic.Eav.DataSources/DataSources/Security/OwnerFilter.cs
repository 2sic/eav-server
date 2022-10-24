﻿using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Logging;
using ToSic.Lib.Documentation;
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
        Icon = Icons.PersonCircled,
        Type = DataSourceType.Security,
        GlobalName = "ToSic.Eav.DataSources.OwnerFilter, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { Constants.DefaultStreamNameRequired },
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
        
        private IImmutableList<IEntity> GetList()
        {
            var wrapLog = Log.Fn<IImmutableList<IEntity>>();

            Configuration.Parse();

            Log.A($"get for identity:{Identity}");
            if (string.IsNullOrWhiteSpace(Identity)) 
                return wrapLog.Return(ImmutableArray<IEntity>.Empty, "no identity");

            if (!GetRequiredInList(out var originals))
                return wrapLog.Return(originals, "error");
            
            return wrapLog.ReturnAsOk(originals.Where(e => e.Owner == Identity).ToImmutableArray());
        }

	}
}