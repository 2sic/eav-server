using System;
using System.IO;

namespace ToSic.Eav.WebApi.Plumbing
{
    public abstract class ResponseMaker<THttpResponseType>
    {
        public abstract THttpResponseType InternalServerError(string message);
        public abstract THttpResponseType InternalServerError(Exception exception);
        
        public abstract THttpResponseType Error(int statusCode, string message);
        
        public abstract THttpResponseType Error(int statusCode, Exception exception);

        public abstract THttpResponseType Json(object json);

        public abstract THttpResponseType Ok();

        public abstract THttpResponseType GetAttachmentHttpResponseMessage(string fileName, string fileType,
            Stream fileContent);

        public abstract THttpResponseType GetAttachmentHttpResponseMessage(string fileName, string fileType,
            string fileContent);

        public abstract THttpResponseType BuildDownload(string content, string fileName);
    }
}
