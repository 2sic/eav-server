using System;
using System.IO;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.WebApi.Infrastructure
{
    public class ResponseMakerUnknown<THttpResponseType> : ResponseMaker<THttpResponseType>
    {
        public ResponseMakerUnknown(WarnUseOfUnknown<ResponseMakerUnknown<THttpResponseType>> _) { }

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

        public override THttpResponseType File(Stream fileContent, string fileName, string fileType)
        {
            throw new NotImplementedException();
        }

        public override THttpResponseType File(string fileContent, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
