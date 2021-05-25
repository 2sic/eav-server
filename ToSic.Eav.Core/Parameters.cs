using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav
{
    [PrivateApi]
    public class Parameters
    {
        #region Parameter protection

        // Special constant to protect functions which should use named parameters
        public const string Protector = "all params must be named, like 'enable: true, language: ''de-ch'' - see https://docs.2sxc.org/net-code/conventions/named-parameters.html";

        // ReSharper disable once UnusedParameter.Local

        public static void ProtectAgainstMissingParameterNames(string criticalParameter, string protectedMethod, string paramNames)
        {
            if (criticalParameter == null || criticalParameter != Protector)
                throw new Exception($"when using '{protectedMethod}' you must use named parameters " +
                                    "- otherwise you are relying on the parameter order staying the same. " +
                                    $"this command experts params like {paramNames}");
        }

        #endregion
    }
}
