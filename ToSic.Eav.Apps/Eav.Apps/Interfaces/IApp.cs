﻿using ToSic.Eav.Apps.State;
using ToSic.Eav.Context;
using ToSic.Eav.LookUp;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps;

/// <summary>
/// An App in memory - for quickly getting things done with the app data, queries etc.
/// </summary>
[PublicApi]
public interface IApp : IAppIdentity, IHasMetadata
{
    /// <summary>
    /// App Name
    /// </summary>
    /// <returns>The name as configured in the app configuration.</returns>
    string Name { get; }

    /// <summary>
    /// App Folder
    /// </summary>
    /// <returns>The folder as configured in the app configuration.</returns>
    string Folder { get; }

    /// <summary>
    /// NameId of the App - usually a string-GUID
    /// </summary>
    string NameId { get; }

    [PrivateApi]
    [Obsolete("Don't use any more, use NameId instead, will be removed ca. v14")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    string AppGuid { get; }

    /// <summary>
    /// Data of the app
    /// </summary>
    IAppData Data { get; }

    /// <summary>
    /// The app metadata - like settings, resources etc.
    /// </summary>
    /// <returns>A metadata provider for the app</returns>
    new IMetadataOf Metadata { get; }


    #region Experimental / new


    /// <summary>
    /// The tenant this app belongs to - for example, a DNN portal
    /// </summary>
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("Don't use any more, use NameId instead, will be removed ca. v17")]
    ISite Site { get; }

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("Don't use any more, use NameId instead, will be removed ca. v14")]
    // TODO: MARK as #Deprecated and log access
    ILookUpEngine ConfigurationProvider { get; }


    // 2024-08-21 2dm - commented out now, for 18.01
    ///// <summary>
    ///// The stored / cached, read-only App State
    ///// </summary>
    //[PrivateApi("Was public till 14.7 but probably never communicated / used except internally. Made Private again in 15.06. Till then was AppState, not interface")]
    //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    //[Obsolete("Don't use any more, use NameId instead, will be removed ca. v14")]
    //IAppState AppState { get; }

    #endregion
}

public interface IAppWithInternal : IApp
{
    IAppReader AppReader { get; }
}