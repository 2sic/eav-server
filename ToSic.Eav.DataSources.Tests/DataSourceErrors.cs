﻿namespace ToSic.Eav.DataSourceTests;

// NOTE: ALREADY COPIED TO THE NEW TEST PROJECT!

public static class DataSourceErrors
{
    /// <summary>
    /// Verify that the source returns an error as expected
    /// </summary>
    /// <param name="source"></param>
    /// <param name="errTitle"></param>
    /// <param name="streamName"></param>
    public static void VerifyStreamIsError(IDataSource source, string errTitle, string streamName = DataSourceConstants.StreamDefaultName)
    {
        IsNotNull(source);
        var stream = source[streamName];
        IsNotNull(stream);
        AreEqual(1, stream.List.Count());
        var firstAndOnly = stream.List.FirstOrDefault();
        IsNotNull(firstAndOnly);
        AreEqual(DataConstants.ErrorTypeName, firstAndOnly.Type.Name);
        AreEqual(DataSourceErrorHelper.GenerateTitle(errTitle), firstAndOnly.GetBestTitle());
    }

}