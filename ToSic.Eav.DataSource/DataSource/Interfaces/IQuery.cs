﻿using ToSic.Eav.DataSource.Sys.Query;

namespace ToSic.Eav.DataSource;

/// <summary>
/// Marks a special <see cref="IDataSource"/> which is a query.
/// It has an underlying <see cref="QueryDefinition"/> and Params which can be modified by code before running the query. 
/// </summary>
[PublicApi]
public interface IQuery: IDataSource, IDataSourceReset
{
    /// <summary>
    /// The underlying definition for the current query so you can check what's inside.
    /// </summary>
    QueryDefinition Definition { get; }

    /// <summary>
    /// Add/Set a parameter for the query, which will be used by the [Params:Xxx] tokens.
    /// </summary>
    /// <param name="key">Key - the part used in [Params:key]</param>
    /// <param name="value">The value it will resolve to. Can also be another token.</param>
    /// <remarks>If you set a param after accessing the query, an exception will occur unless you call Reset() first.</remarks>
    void Params(string key, string? value);

    /// <summary>
    /// Add/Set a parameter for the query, which will be used by the [Params:Xxx] tokens.
    /// Takes any value object and will simply ToString() it.
    /// </summary>
    /// <param name="key">Key - the part used in [Params:key]</param>
    /// <param name="value">The value it will resolve to. Can also be another token.</param>
    /// <remarks>
    /// - If you set a param after accessing the query, an exception will occur unless you call Reset() first.
    /// - History: Added in v15
    /// </remarks>
    void Params(string key, object? value);

    /// <summary>
    /// Add/Set a parameter for the query, which will be used by the [Params:Xxx] tokens.
    /// </summary>
    /// <param name="list">list of key=value on many lines</param>
    /// <remarks>If you set a param after accessing the query, an exception will occur unless you call Reset() first.</remarks>
    void Params(string list);

    /// <summary>
    /// Add/Set a parameter for the query, which will be used by the [Params:Xxx] tokens.
    /// </summary>
    /// <param name="values">dictionary with values</param>
    /// <remarks>If you set a param after accessing the query, an exception will occur unless you call Reset() first.</remarks>
    void Params(IDictionary<string, string> values);

    /// <summary>
    /// Get the current list of params.
    /// </summary>
    /// <returns>The list of params as they are configured in this moment.</returns>
    IDictionary<string, string> Params();


    ///// <summary>
    ///// Inner source - mainly for debugging etc.
    ///// </summary>
    //[PrivateApi]
    //IDataSource Source { get; }
}