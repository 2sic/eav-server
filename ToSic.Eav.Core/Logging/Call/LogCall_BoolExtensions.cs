namespace ToSic.Eav.Logging
{
    public static class LogCall_BoolExtensions
    {

        // TODO: @STV - pls use this instead of most calls where true/  false is in the result

        public static bool ReturnTrue(this LogCall<bool> logCall) => logCall.ReturnTrue("true");

        public static bool ReturnTrue(this LogCall<bool> logCall, string message) => logCall?.Return(true, message) ?? true;

        public static bool ReturnFalse(this LogCall<bool> logCall) => logCall.ReturnFalse("false");

        public static bool ReturnFalse(this LogCall<bool> logCall, string message) => logCall?.Return(false, message) ?? false;

    }
}
