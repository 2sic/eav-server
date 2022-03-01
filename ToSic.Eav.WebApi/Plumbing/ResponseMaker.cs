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

        public abstract THttpResponseType GetAttachmentHttpResponseMessage(string fileName, string fileType,
            Stream fileContent);

        public virtual THttpResponseType GetAttachmentHttpResponseMessage(string fileName, string fileType, string fileContent)
        {
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            return GetAttachmentHttpResponseMessage(fileName, fileType, new MemoryStream(fileBytes));
        }

        public abstract THttpResponseType BuildDownload(string content, string fileName);
    }
}
