﻿using System.Collections.Generic;
using System.Net;
using ToSic.Eav.Security;

namespace ToSic.Eav.WebApi.Errors;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class HttpException
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

    public static HttpExceptionAbstraction BadRequest(string message) => new(HttpStatusCode.BadRequest, message);

    public static HttpExceptionAbstraction InformativeErrorForTypeAccessDenied(string contentType, List<Grants> grant, bool staticNameIsGuid)
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

    public static HttpExceptionAbstraction NotAllowedFileType(string filename, string message = null) 
        => new(HttpStatusCode.UnsupportedMediaType, $"file {filename} has an unsupported file type. {message}");

    public static HttpExceptionAbstraction PermissionDenied(string message = null) 
        => new(HttpStatusCode.Forbidden, $"Permission denied. {message}");

    public static HttpExceptionAbstraction MissingParam(string paramName) 
        => new(HttpStatusCode.BadRequest, $"Param {paramName} missing.");
}