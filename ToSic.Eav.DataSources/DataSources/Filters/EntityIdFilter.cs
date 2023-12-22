using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSource.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

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
    NameId = "ToSic.Eav.DataSources.EntityIdFilter, ToSic.Eav.DataSources",
    DynamicOut = false,
    In = new[] { InStreamDefaultRequired },
    ConfigurationType = "|Config ToSic.Eav.DataSources.EntityIdFilter",
    HelpLink = "https://go.2sxc.org/DsIdFilter")]

public class EntityIdFilter : Eav.DataSource.DataSourceBase
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
            Configuration.SetThisObsolete(cleaned);
        }
    }

    #endregion

    /// <summary>
    /// Constructs a new EntityIdFilter
    /// </summary>
    [PrivateApi]
    public EntityIdFilter(MyServices services): base(services, $"{LogPrefix}.EntIdF")
    {
        ProvideOut(GetList);
    }

    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        var entityIdsOrError = CustomConfigurationParse();
        if (entityIdsOrError.IsError)
            return l.ReturnAsError(entityIdsOrError.Errors);

        var entityIds = entityIdsOrError.Result;

        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

        var result = entityIds.Select(eid => source.One(eid)).Where(e => e != null).ToImmutableList();

        l.A(l.Try(() => $"get ids:[{string.Join(",", entityIds)}] found:{result.Count}"));
        return l.ReturnAsOk(result);
    }

    [PrivateApi]
    private ResultOrError<int[]> CustomConfigurationParse()
    {
        var l = Log.Fn<ResultOrError<int[]>>();
        Configuration.Parse();

        #region clean up list of IDs to remove all white-space etc.

        try
        {
            var configEntityIds = EntityIds;
            // check if we have anything to work with
            if (string.IsNullOrWhiteSpace(configEntityIds))
                return l.Return(new ResultOrError<int[]>(true, Array.Empty<int>()), "empty");

            var preCleanedIds = configEntityIds
                .Split(',')
                .Where(strEntityId => !string.IsNullOrWhiteSpace(strEntityId));
            var lstEntityIds = new List<int>();
            foreach (var strEntityId in preCleanedIds)
                if (int.TryParse(strEntityId, out var entityIdToAdd))
                    lstEntityIds.Add(entityIdToAdd);
            return l.Return(new ResultOrError<int[]>(true, lstEntityIds.Distinct().ToArray()), EntityIds);
        }
        catch (Exception ex)
        {
            return l.ReturnAsError(new ResultOrError<int[]>(false, Array.Empty<int>(),
                Error.Create(title: "Can't find IDs", message: "Unable to load EntityIds from Configuration. Unexpected Exception.",
                    exception: ex)));
        }

        #endregion
    }

}