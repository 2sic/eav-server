using System;
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
    }
}
