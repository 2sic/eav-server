namespace ToSic.Eav.WebApi.ApiExplorer
{
    public interface IApiExplorerController<THttpResponseType>
    {
        THttpResponseType Inspect(string path);
    }
}