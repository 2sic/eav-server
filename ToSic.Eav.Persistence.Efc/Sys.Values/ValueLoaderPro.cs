﻿using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.Data.Sys.Dimensions;
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Eav.Persistence.Efc.Sys.Entities;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Persistence.Efc.Sys.TempModels;

namespace ToSic.Eav.Persistence.Efc.Sys.Values;

internal class ValueLoaderPro(EfcAppLoaderService appLoader, EntityDetailsLoadSpecs specs)
    : ValueLoaderStandard(appLoader, specs, "Efc.VlLdPr")
{
    [field: AllowNull, MaybeNull]
    private ValueQueriesPro ValueQueries => field ??= new(AppLoader, Log);

    public override Dictionary<int, ICollection<TempAttributeWithValues>> LoadValues()
    {
        var l = Log.IfDetails(AppLoader.LogSettings).Fn<Dictionary<int, ICollection<TempAttributeWithValues>>>($"LoadAll: {Specs.LoadAll}", timer: true);

        // Check if we should use the optimized code, which only works for loading everything
        if (!Specs.LoadAll)
        {
            l.A("Don't load all, use default chunk loader");
            var partialLoad = base.LoadValues();
            return l.Return(partialLoad, "Partial load");
        }

        var sqlTime = Stopwatch.StartNew();
        var result = GetValuesOfAllEntitiesInApp(appId: Specs.AppId);
        sqlTime.Stop();

        var attributes = result.ToDictionary(i => i.Key, i => i.Value);

        AppLoader.AddSqlTime(sqlTime.Elapsed);

        return l.Return(attributes, $"Found {attributes.Count} attributes");
    }

    #region Get All Entities

    private Dictionary<int, ICollection<TempAttributeWithValues>> GetValuesOfAllEntitiesInApp(int appId)
    {
        var l = Log.Fn<Dictionary<int, ICollection<TempAttributeWithValues>>>($"appId:{appId}", timer: true);

        var attributesRaw = GetSqlValuesAll(appId);

        var attributes = ConvertToTempAttributes(attributesRaw);
        return l.ReturnAsOk(attributes);
    }

    /// <summary>
    /// Get the attributes of the entities we're loading.
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    private ICollection<LoadingValue> GetSqlValuesAll(int appId)
    {
        var l = Log.Fn<ICollection<LoadingValue>>($"Attributes SQL for appId:{appId}", timer: true);

        var query = ValueQueries.AllValuesQuery(appId);
        var attributesRaw = ToLoadingValues(query.Query, query.Dimensions);

        return l.Return(attributesRaw, $"found {attributesRaw.Count} attributes");
    }

    /// <summary>
    /// Method to convert query results to LoadingValue list.
    /// </summary>
    /// <param name="values"></param>
    /// <param name="dimensions"></param>
    /// <remarks>
    /// It's really important that we use an IQueryable here, because otherwise the
    /// SQL isn't properly converted, and we'll run into null exceptions! So don't use IEnumerable,
    /// at least not for EF 2.2 which is still in use for DNN.
    /// </remarks>
    internal override ICollection<LoadingValue> ToLoadingValues(IQueryable<TsDynDataValue> values, List<TsDynDataDimension>? dimensions)
    {
        return values
            .Select(v => new LoadingValue(
                v.EntityId,
                v.AttributeId,
                v.Attribute.StaticName,
                v.Value,
                v.TsDynDataValueDimensions
                    .Select(ILanguage (lng) =>
                        new Language(lng.Dimension.EnvironmentKey! /* is never null on a non-root culture */, lng.ReadOnly, lng.DimensionId))
                    .ToList() // ToList is an important optimization, ask 2dm. Do NOT change to ToListOpt (which is an EAV extension, not EF Core compatible!),
            ))
            .ToListOpt();
    }

    #endregion

    #region Experimental possibly faster way combining Dimensions manually

    /// <summary>
    /// Method to convert query results to LoadingValue list.
    /// </summary>
    /// <param name="values"></param>
    /// <param name="dimensions"></param>
    /// <remarks>
    /// It's really important that we use an IQueryable here, because otherwise the
    /// SQL isn't properly converted and we'll run into null exceptions! So don't use IEnumerable,
    /// at least not for EF 2.2 which is still in use for DNN.
    /// </remarks>
    private List<LoadingValue> ToLoadingValuesAll2Steps(
        IQueryable<TsDynDataValue> values, // It's very important that we use an IQueryable here! see remarks.
        List<TsDynDataDimension> dimensions
    )
    {
        var lookup = dimensions
            .ToDictionary(d => d.DimensionId, d => d.EnvironmentKey);

        var temp = values
            .Select(v => new
            {
                v.EntityId,
                v.AttributeId,
                v.Attribute.StaticName,
                v.Value,
                TsDynDataValueDimensions = v.TsDynDataValueDimensions
                    .Select(lng => new
                    {
                        // lng.Dimension.EnvironmentKey,
                        lng.ReadOnly,
                        lng.DimensionId
                    })
                    .ToList()
            })
            .ToList();

        // Convert to LoadingValue
        var final = temp.Select(v => new LoadingValue(
                v.EntityId,
                v.AttributeId,
                v.StaticName,
                v.Value,
                v.TsDynDataValueDimensions
                    .Select(ILanguage (lng) =>
                        new Language(lookup[lng.DimensionId]! /* would only be null on the root entry, never on lower ones */, lng.ReadOnly, lng.DimensionId))
                    .ToList()
            ))
            .ToList();
        return final;

    }

    #endregion

    #region Test
    // 2025-05-12, stv, test alternative strategy to load values
    //private List<LoadingValue> GetSqlValues2(List<int> entityIdsFound)
    //{
    //    var l = Log.Fn<List<LoadingValue>>($"Attributes SQL for {entityIdsFound.Count} entities", timer: true);

    //    var attributesRaw = ValueQueries
    //        .ValuesOfIdsQueryOptimized2(entityIdsFound)
    //        .Select(v => new LoadingValue(
    //                v.EntityId,
    //                v.AttributeId,
    //                v.StaticName,
    //                v.Value,

    //                v.ValueDimensionResults
    //                    .Select(lng =>
    //                        new Language(lng.EnvironmentKey, lng.ReadOnly, lng.DimensionId) as ILanguage)
    //                    .ToImmutableList()
    //            )
    //        )
    //        .ToList();

    //    return l.Return(attributesRaw, $"found {attributesRaw.Capacity} attributes");
    //}

    #endregion
}