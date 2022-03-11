using System;
using System.IO;
using System.Net;
using System.Text;

namespace ToSic.Eav.WebApi.Plumbing
{
    public abstract class ResponseMaker<THttpResponseType>
    {
        public virtual THttpResponseType InternalServerError(string message)
            => Error((int)HttpStatusCode.InternalServerError, message);

        public virtual THttpResponseType InternalServerError(Exception exception)
            => Error((int)HttpStatusCode.InternalServerError, exception);

        public abstract THttpResponseType Error(int statusCode, string message);
        
        public abstract THttpResponseType Error(int statusCode, Exception exception);

        public abstract THttpResponseType Json(object json);

        public abstract THttpResponseType Ok();

        public abstract THttpResponseType File(Stream fileContent, string fileName, string fileType);

        public virtual THttpResponseType File(string fileContent, string fileName, string fileType)
        {
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            return File(new MemoryStream(fileBytes), fileName, fileType);
        }

        public abstract THttpResponseType File(string fileContent, string fileName);
    }
}
