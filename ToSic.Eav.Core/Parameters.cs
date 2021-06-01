using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav
{
    [PrivateApi]
    public class Parameters
    {
        #region Parameter protection

        // Special constant to protect functions which should use named parameters
        public const string Protector = "Rule: all params must be named (https://r.2sxc.org/named-params), Example: \"enable: true, language: ''de-ch''\"";

        // ReSharper disable once UnusedParameter.Local

        public static void ProtectAgainstMissingParameterNames(string criticalParameter, string protectedMethod, string paramNames)
        {
            if (criticalParameter == null || criticalParameter != Protector)
                throw new Exception($"when using '{protectedMethod}' you must use named parameters " +
                                    "- otherwise you are relying on the parameter order staying the same. " +
                                    $"this command expects these parameters: {paramNames}");
        }

        #endregion
    }
}
