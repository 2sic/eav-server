using System;

namespace ToSic.Sxc.WebApi.Plumbing
{
    public abstract class ResponseMaker<HttpResponseType>
    {
        public abstract HttpResponseType InternalServerError(string message);
        public abstract HttpResponseType InternalServerError(Exception exception);
        
        public abstract HttpResponseType Error(int statusCode, string message);
        
        public abstract HttpResponseType Error(int statusCode, Exception exception);
        public abstract HttpResponseType Json(object json);
    }
}
