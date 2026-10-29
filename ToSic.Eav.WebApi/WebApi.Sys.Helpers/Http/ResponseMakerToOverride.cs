using System.Net;
#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.WebApi.Sys.Helpers.Http;

/// <summary>
/// Dummy / unknown implementation, which must usually be overridden before it becomes available.
/// </summary>
/// <param name="_"></param>
internal class ResponseMakerToOverride(WarnUseOfUnknown<ResponseMakerToOverride> _) : IResponseMaker
{
    public void Init(TDotNetController controller)
    {
        // do nothing
    }

    public virtual THttpResponseType InternalServerError(string message)
        => Error((int)HttpStatusCode.InternalServerError, message);

    public virtual THttpResponseType InternalServerError(Exception exception)
        => Error((int)HttpStatusCode.InternalServerError, exception);

    [DoesNotReturn]
    public THttpResponseType Throw() => throw new NotImplementedException();

    public THttpResponseType Error(int statusCode, string message) => Throw();

    public THttpResponseType Error(int statusCode, Exception exception) => Throw();

    public THttpResponseType Json(object json) => Throw();

    public THttpResponseType Ok() => Throw();

    public THttpResponseType File(Stream fileContent, string fileName, string fileType) => Throw();

    public THttpResponseType File(string fileContent, string fileName, string fileType) => Throw();

    public THttpResponseType File(string fileContent, string fileName) => Throw();
}
