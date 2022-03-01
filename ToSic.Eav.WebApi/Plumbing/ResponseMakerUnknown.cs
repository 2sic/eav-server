using System;
using System.IO;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.WebApi.Plumbing
{
    public class ResponseMakerUnknown<THttpResponseType> : ResponseMaker<THttpResponseType>
    {
        public ResponseMakerUnknown(WarnUseOfUnknown<ResponseMakerUnknown<THttpResponseType>> warn) { }

        public override THttpResponseType InternalServerError(string message)
        {
            throw new NotImplementedException();
        }

        public override THttpResponseType InternalServerError(Exception exception)
        {
            throw new NotImplementedException();
        }

        public override THttpResponseType Error(int statusCode, string message)
        {
            throw new NotImplementedException();
        }

        public override THttpResponseType Error(int statusCode, Exception exception)
        {
            throw new NotImplementedException();
        }

        public override THttpResponseType Json(object json)
        {
            throw new NotImplementedException();
        }

        public override THttpResponseType Ok()
        {
            throw new NotImplementedException();
        }

        public override THttpResponseType GetAttachmentHttpResponseMessage(string fileName, string fileType, Stream fileContent)
        {
            throw new NotImplementedException();
        }

        public override THttpResponseType GetAttachmentHttpResponseMessage(string fileName, string fileType, string fileContent)
        {
            throw new NotImplementedException();
        }

        public override THttpResponseType BuildDownload(string content, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
