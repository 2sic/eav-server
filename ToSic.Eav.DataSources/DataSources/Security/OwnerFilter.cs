﻿using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSource.DataSourceConstants;
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
        NameId = "ToSic.Eav.DataSources.OwnerFilter, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { InStreamDefaultRequired },
	    ConfigurationType = "|Config ToSic.Eav.DataSources.OwnerFilter",
        HelpLink = "https://go.2sxc.org/DsOwnerFilter")]

    public class OwnerFilter : Eav.DataSource.DataSourceBase
	{
        #region Configuration-properties

        private const string IdentityCodeField = "IdentityCode";

        /// <summary>
        /// The identity of the user to filter by. Uses the Identity-token convention like dnn:1 is the user #1 in the DNN DB
        /// </summary>
        [Configuration(Field = IdentityCodeField)]
        public string Identity
        {
            get => Configuration.GetThis();
            set => Configuration.SetThisObsolete(value);
        }
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new PublishingFilter
		/// </summary>
		[PrivateApi]
		public OwnerFilter(MyServices services): base(services, $"{LogPrefix}.OwnrFl")
        {
            ProvideOut(GetList);
        }

        private IImmutableList<IEntity> GetList() => Log.Func($"get for identity:{Identity}", () =>
        {
            Configuration.Parse();

            if (string.IsNullOrWhiteSpace(Identity))
                return (EmptyList, "no identity");

            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(), "error");

            return (source.Where(e => e.Owner == Identity).ToImmutableList(), "ok");
        });

    }
}