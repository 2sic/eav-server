﻿using System.Collections;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource.Sys.Caching;
using ToSic.Eav.DataSource.Sys.Configuration;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSource;

/// <summary>
/// Very lightweight DataSource base for data sources which are very simple and convention based.
/// </summary>
[PublicApi]
public class CustomDataSource: CustomDataSourceAdvanced
{
    /// <summary>
    /// The Services of CustomDataSource.
    /// Note that it is the same as the base MyServices,
    /// but it's still important to have an own class.
    /// This is in case some day it will need more dependencies.
    /// Otherwise, compiled code would break when we need additional dependencies just for the CustomDataSource.
    /// </summary>
    [PrivateApi]
    public new class Dependencies(
        IDataSourceConfiguration configuration,
        LazySvc<DataSourceErrorHelper> errorHandler,
        ConfigurationDataLoader configDataLoader,
        LazySvc<IDataSourceCacheService> cacheService,
        IDataFactory dataFactory
        )
        : CustomDataSourceAdvanced.Dependencies(configuration, errorHandler, configDataLoader, cacheService, dataFactory);

    /// <summary>
    /// Constructor for creating a Custom DataSource.
    /// </summary>
    /// <param name="services">All the needed services - see [](xref:NetCode.Conventions.MyServices)</param>
    /// <param name="logName">Optional name for logging such as `My.JsonDS`</param>
    protected internal CustomDataSource(Dependencies services, string? logName = null) : base(services, logName ?? "Ds.CustLt")
    {
        // Provide a default out, in case the overriding class doesn't
        base.ProvideOut(() => GetRaw(GetDefault, null));
    }

    /// <summary>
    /// Every new DataSource based on this is [immutable](xref:NetCode.Conventions.Immutable).
    /// </summary>
    public override bool Immutable => true;

    [field: AllowNull, MaybeNull]
    private DataFactoryOptions Options
    {
        get => field ??= new() { TypeName = "Custom" };
        set;
    }

    protected virtual IEnumerable<IRawEntity> GetDefault() => new List<IRawEntity>();

    // #DropIHasRawEntity v20
    ///// * <see cref="IHasRawEntity{T}"/>

    /// <summary>
    /// Provide data on the `Out` of this DataSource.
    /// This is a very generic version which takes any function that generates a list of something.
    /// Internally it will try to detect what the data was and convert it to the final format.
    ///
    /// Note that the `source` must create a list (`IEnumerable`) of any of the following (all items must have the same type):
    /// * <see cref="IEntity"/>
    /// * <see cref="IRawEntity"/>
    ///
    /// If you know what data type you're creating, you should look at the other ProvideOut* methods.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="name">_optional_ name of the out-stream.</param>
    /// <param name="options">Conversion options which are relevant for <see cref="IRawEntity"/> data</param>
    protected internal void ProvideOut(
        Func<object> data,
        NoParamOrder noParamOrder = default,
        string name = StreamDefaultName,
        Func<DataFactoryOptions>? options = default) =>
        base.ProvideOut(() => GetAny(data, options), name);

    // #DropIHasRawEntity v20
    //[PrivateApi]
    //protected internal void ProvideOutRaw<T>(
    //    Func<IEnumerable<IHasRawEntity<T>>> data,
    //    NoParamOrder noParamOrder = default,
    //    string name = StreamDefaultName,
    //    Func<DataFactoryOptions>? options = default) where T : IRawEntity =>
    //    base.ProvideOut(() => GetHasRaw(data, options), name);

    [PrivateApi]
    protected internal void ProvideOutRaw<T>(
        Func<IEnumerable<T>> data,
        NoParamOrder noParamOrder = default,
        string name = StreamDefaultName,
        Func<DataFactoryOptions>? options = default) where T : IRawEntity =>
        base.ProvideOut(() => GetRaw(data, options), name);

    private IImmutableList<IEntity> GetAny(Func<object>? source, Func<DataFactoryOptions>? options)
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        // Call the Generator and handle errors/null
        object? funcResult;
        try
        {
            funcResult = source?.Invoke();
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            var runErr = Error.Create(title: $"Error calling source generator of {nameof(ProvideOut)}. " +
                                             "Error details can be found in Insights.", exception: ex);
            return l.ReturnAsError(runErr);
        }
        if (funcResult is null)
            return l.Return([], "null, no data returned");

        // Make a list out of the result
        List<object> data;
        try
        {
            data = funcResult is IEnumerable enumerable
                ? enumerable.Cast<object>().ToList()
                : [funcResult];
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            var runErr = Error.Create(title: $"Error handling result of source generator of {nameof(ProvideOut)}. " +
                                             "Error details can be found in Insights.", exception: ex);
            return l.ReturnAsError(runErr);
        }

        // Handle empty list
        if (data.SafeNone())
            return l.Return([], "no items returned");

        // Handle all is already converted to IEntity
        if (data.All(i => i is IEntity))
            return l.Return(data.Cast<IEntity>().ToImmutableOpt(), "IEntities");

        // If all are Anonymous, convert to Raw
        if (data.All(d => d.IsAnonymous()))
        {
            l.A("Was anonymous, converted to raw");
            data = data.Select(d => new RawFromAnonymous(d, Log)).Cast<object>().ToList();
        }

        // Handle raw
        if (data.All(i => i is IRawEntity))
        {
            var raw = data.Cast<IRawEntity>().ToList();
            var result = DataFactory.SpawnNew(options: GetBest(options)).Create(raw);
            return l.Return(result, "was IRawEntity");
        }

        // #DropIHasRawEntity v20
        // Do this first, to make all the data be IRawEntity
        //if (data.All(i => i is IHasRawEntity))
        //{
        //    var raw = data.Cast<IHasRawEntity<IRawEntity>>().ToList();
        //    var result = DataFactory.SpawnNew(options: GetBest(options)).Create(raw);
        //    return l.Return(result, "was IHasRawEntity");
        //}

        // todo - maybe also process IHasEntity - but only after doing the raw entities

        var err = Error.Create(title: $"Error in {nameof(ProvideOutRaw)}",
            message: "The list received was tested against all possible data types but non matched. " +
                     // #DropIHasRawEntity v20
                     //$"Expected was a list of either {nameof(IEntity)}, {nameof(IRawEntity)}, {nameof(IHasRawEntity<IRawEntity>)}. " +
                     $"Expected was a list of either {nameof(IEntity)}, {nameof(IRawEntity)}. " +
                     "Note that all items must be of the same type. ");
        return l.ReturnAsError(err);
    }

    private DataFactoryOptions GetBest(Func<DataFactoryOptions>? options)
        => options?.Invoke() ?? Options;

    private IImmutableList<IEntity> GetRaw<T>(Func<IEnumerable<T>>? source, Func<DataFactoryOptions>? options)
        where T: IRawEntity
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        // Get raw entities - from _source or from override method
        var raw = source?.Invoke()?.ToList();

        // If we didn't get anything, return empty
        if (raw.SafeNone())
            return l.Return([], "no items returned");

        // Transform result to IEntity
        var result = DataFactory.SpawnNew(options: GetBest(options)).Create(raw);
        return l.Return(result, $"Got {result.Count} items");
    }

    // #DropIHasRawEntity v20
    //private IImmutableList<IEntity> GetHasRaw<T>(Func<IEnumerable<IHasRawEntity<T>>>? source, Func<DataFactoryOptions>? options)
    //    where T: IRawEntity
    //{
    //    var l = Log.Fn<IImmutableList<IEntity>>();
    //    Configuration.Parse();

    //    // Get raw entities - from _source or from override method
    //    var raw = source?.Invoke()?.ToList();

    //    // If we didn't get anything, return empty
    //    if (raw.SafeNone())
    //        return l.Return([], "no items returned");

    //    // Transform result to IEntity
    //    var result = DataFactory.SpawnNew(options: GetBest(options)).Create(raw);
    //    return l.Return(result, $"Got {result.Count} items");
    //}


}