using System;
using System.IO;
using System.Net;
using ToSic.Eav.Internal.Unknown;

#if NETFRAMEWORK
using THttpResponseType = System.Net.Http.HttpResponseMessage;
using TController = System.Web.Http.ApiController;
#else
using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
using TController = Microsoft.AspNetCore.Mvc.ControllerBase;
#endif

namespace ToSic.Eav.WebApi.Infrastructure;

internal class ResponseMakerUnknown : IResponseMaker
{
    public ResponseMakerUnknown(WarnUseOfUnknown<ResponseMakerUnknown> _) { }

    public void Init(TController controller)
    {
        // do nothing
    }

    public virtual THttpResponseType InternalServerError(string message)
        => Error((int)HttpStatusCode.InternalServerError, message);

    public virtual THttpResponseType InternalServerError(Exception exception)
        => Error((int)HttpStatusCode.InternalServerError, exception);


    public THttpResponseType Error(int statusCode, string message)
    {
        throw new NotImplementedException();
    }

    public THttpResponseType Error(int statusCode, Exception exception)
    {
        throw new NotImplementedException();
    }

    public THttpResponseType Json(object json)
    {
        throw new NotImplementedException();
    }

    public THttpResponseType Ok()
    {
        throw new NotImplementedException();
    }

    public THttpResponseType File(Stream fileContent, string fileName, string fileType)
    {
        throw new NotImplementedException();
    }

    public THttpResponseType File(string fileContent, string fileName, string fileType)
    {
        throw new NotImplementedException();
    }

    public THttpResponseType File(string fileContent, string fileName)
    {
        throw new NotImplementedException();
    }
}
