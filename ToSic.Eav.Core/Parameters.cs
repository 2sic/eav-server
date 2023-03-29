using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;

namespace ToSic.Eav
{
    [PrivateApi]
    public class Parameters
    {
        #region Parameter protection

        /// <summary>
        /// This is a special constant to protect functions which should use named parameters.
        /// It is used in method signatures like Method(string expectedParam, string noParamOrder = Parameters.Protector, object nextParam = null, ...)
        /// </summary>
        /// <remarks>
        /// It must be a const, otherwise it couldn't be used in method signatures :(
        /// </remarks>
        public const string Protector = "Params must be named (https://r.2sxc.org/named-params)";

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
            if (criticalParameter == null || criticalParameter != Protector)
                throw new Exception($"when using '{protectedMethod}' you must use named parameters " +
                                    "- otherwise you are relying on the parameter order staying the same. " +
                                    "See https://r.2sxc.org/named-params " +
                                    $"This command expects these parameters: {paramNames}");
        }

        public static void Protect(
            string criticalParameter, 
            string paramNames = null,
            [CallerMemberName] string methodName = null)
        {
            ProtectAgainstMissingParameterNames(criticalParameter, methodName, paramNames);
        }

        #endregion
    }
}
