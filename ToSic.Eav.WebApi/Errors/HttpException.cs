using System.Collections.Generic;
using System.Net;
using ToSic.Eav.Security;

namespace ToSic.Eav.WebApi.Errors
{
    internal class HttpException
    {
        /// <summary>
        /// Throw a correct HTTP error with the right error-numbr. This is important for the JavaScript which changes behavior & error messages based on http status code
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="message"></param>
        /// <param name="tags"></param>
        private static HttpExceptionAbstraction WithLink(HttpStatusCode httpStatusCode, string message, string tags = "")
        {
            var helpText = message + " See http://2sxc.org/help" + (tags == "" ? "" : "?tag=" + tags);
            return new HttpExceptionAbstraction(httpStatusCode, helpText);
        }

        internal static HttpExceptionAbstraction BadRequest(string message)
            => new HttpExceptionAbstraction(HttpStatusCode.BadRequest, message);

        internal static HttpExceptionAbstraction InformativeErrorForTypeAccessDenied(string contentType, List<Grants> grant, bool staticNameIsGuid)
        {
            var grantCodes = string.Join(",", grant);

            // if the cause was not-admin and not testable, report better error
            if (!staticNameIsGuid)
                return WithLink(HttpStatusCode.Unauthorized,
                    "Content Type '" + contentType + "' is not a standard Content Type - no permissions possible.");

            // final case: simply not allowed
            return WithLink(HttpStatusCode.Unauthorized,
                "Request not allowed. User needs permissions to " + grantCodes + " for Content Type '" + contentType + "'.",
                "permissions");
        }

        internal static HttpExceptionAbstraction NotAllowedFileType(string filename, string message = null) 
            => new HttpExceptionAbstraction(HttpStatusCode.UnsupportedMediaType, $"file {filename} has an unsupported file type. {message}");

        internal static HttpExceptionAbstraction PermissionDenied(string message = null) 
            => new HttpExceptionAbstraction(HttpStatusCode.Forbidden, $"Permission denied. {message}");

        internal static HttpExceptionAbstraction MissingParam(string paramName) 
            => new HttpExceptionAbstraction(HttpStatusCode.BadRequest, $"Param {paramName} missing.");
    }
}
