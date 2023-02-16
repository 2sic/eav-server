using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that filters Entities by Ids. Can handle multiple IDs if comma-separated.
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Item Id Filter",
        UiHint = "Find items based on one or more IDs",
        Icon = Icons.Fingerprint,
        Type = DataSourceType.Filter, 
        GlobalName = "ToSic.Eav.DataSources.EntityIdFilter, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { Constants.DefaultStreamNameRequired },
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.EntityIdFilter",
        HelpLink = "https://r.2sxc.org/DsIdFilter")]

    public class EntityIdFilter : DataSource
	{
        #region Configuration-properties

        /// <summary>
        /// A string containing one or more entity-ids. like "27" or "27,40,3063,30306"
        /// </summary>
        [Configuration]
		public string EntityIds
        {
            get => Configuration.GetThis();
            set
            {
                // kill any spaces in the string
                var cleaned = Regex.Replace(value ?? "", @"\s+", "");
                Configuration.SetThis(cleaned);
            }
        }

        #endregion

		/// <summary>
		/// Constructs a new EntityIdFilter
		/// </summary>
		[PrivateApi]
		public EntityIdFilter(Dependencies services): base(services, $"{DataSourceConstants.LogPrefix}.EntIdF")
        {
            Provide(GetList);
		}

        private IImmutableList<IEntity> GetList() => Log.Func(l =>
        {
            var entityIds = CustomConfigurationParse();

            // if CustomConfiguration resulted in an error, report now
            if (!ErrorStream.IsDefaultOrEmpty)
                return (ErrorStream, "error");

            if (!GetRequiredInList(out var originals))
                return (originals, "error");

            var result = entityIds.Select(eid => originals.One(eid)).Where(e => e != null).ToImmutableArray();

            l.A(l.Try(() => $"get ids:[{string.Join(",", entityIds)}] found:{result.Length}"));
            return (result, "ok");
        });

        [PrivateApi]
        private int[] CustomConfigurationParse() => Log.Func(() =>
        {
            Configuration.Parse();

            #region clean up list of IDs to remove all white-space etc.

            try
            {
                var configEntityIds = EntityIds;
                // check if we have anything to work with
                if (string.IsNullOrWhiteSpace(configEntityIds))
                    return (Array.Empty<int>(), "empty");

                var preCleanedIds = configEntityIds
                    .Split(',')
                    .Where(strEntityId => !string.IsNullOrWhiteSpace(strEntityId));
                var lstEntityIds = new List<int>();
                foreach (var strEntityId in preCleanedIds)
                    if (int.TryParse(strEntityId, out var entityIdToAdd))
                        lstEntityIds.Add(entityIdToAdd);
                return (lstEntityIds.Distinct().ToArray(), EntityIds);
            }
            catch (Exception ex)
            {
                SetError("Can't find IDs", "Unable to load EntityIds from Configuration. Unexpected Exception.", ex);
                return (null, "error");
            }

            #endregion
        });

    }
}