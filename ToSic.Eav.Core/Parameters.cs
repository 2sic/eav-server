using System;
using System.Runtime.CompilerServices;
using ToSic.Eav.Code;
using ToSic.Lib.Documentation;

namespace ToSic.Eav
{
    [PrivateApi]
    public class Parameters
    {
        public const string HelpLink = "https://go.2sxc.org/named-params";

        /// <summary>
        /// This is a special constant to protect functions which should use named parameters.
        /// It is used in method signatures like Method(string expectedParam, string noParamOrder = Parameters.Protector, object nextParam = null, ...)
        /// </summary>
        /// <remarks>
        /// It must be a const, otherwise it couldn't be used in method signatures :(
        /// </remarks>
        public const string Protector = "Params must be named (" + HelpLink + ")";

        /// <summary>
        /// Parameter checker which should be called on all protected methods. It will help you generate a proper error message if the parameters were not named. 
        /// </summary>
        /// <param name="criticalParameter">The noParamOrder parameter which will be verified that it's the default value.</param>
        /// <param name="protectedMethod">Name of the method we protect (for error messages)</param>
        /// <param name="paramNames">String with param-names to show, usually generated with a bunch of nameof(paramName) </param>
        /// <exception cref="Exception"></exception>
        // TODO: REPLACE MOST calls with the Protect below, which doesn't need the name of the method calling - careful - param order is different
        public static void ProtectAgainstMissingParameterNames(string criticalParameter, string protectedMethod = null, string paramNames = null)
        {
            if (criticalParameter != Protector)
                throw CreateException(protectedMethod, paramNames);
        }

        public static void Protect(
            string criticalParameter, 
            string paramNames = default,
            [CallerMemberName] string methodName = default)
        {
            // Note: this is duplicate code, but we don't want the call stack to become more confusing
            if (criticalParameter != Protector)
                throw CreateException(methodName, paramNames);
        }

        private static NamedArgumentException CreateException(string methodName, string paramNames)
        {
            var intro = $"When using '.{methodName}(...)'\n you must use named parameters\n to ensure your code works in future\n when params might change. ";
            var see = $"See {HelpLink} ";
            var paramsText = paramNames == null ? "" : $". This command expects these parameters: '{paramNames}'.";
            var msg = intro + see + paramsText;
            return new NamedArgumentException(msg, intro, paramNames, paramsText);
        }


    }
}
