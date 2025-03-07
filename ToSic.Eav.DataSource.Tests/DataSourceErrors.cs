namespace ToSic.Eav;

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
        NotNull(source);
        var stream = source[streamName];
        NotNull(stream);
        Single(stream.List);
        var firstAndOnly = stream.List.FirstOrDefault();
        NotNull(firstAndOnly);
        Equal(DataConstants.ErrorTypeName, firstAndOnly.Type.Name);
        Equal(DataSourceErrorHelper.GenerateTitle(errTitle), firstAndOnly.GetBestTitle());
    }

}