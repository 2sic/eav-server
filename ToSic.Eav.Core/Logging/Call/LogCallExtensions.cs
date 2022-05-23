namespace ToSic.Eav.Logging.Call
{
    public static class LogCallExtensions
    {
        // TODO: @STV - pls use this instead of most calls where "ok" is in the text

        public static T ReturnAsOk<T>(this LogCall<T> logCall, T result) => logCall.Return(result, "ok");


        // TODO: @STV - pls use this instead of most calls where true/  false is in the result

        public static bool ReturnTrue(this LogCall<bool> logCall) => logCall.ReturnTrue("true");

        public static bool ReturnTrue(this LogCall<bool> logCall, string message) => logCall?.Return(true, message) ?? true;

        public static bool ReturnFalse(this LogCall<bool> logCall) => logCall.ReturnFalse("false");

        public static bool ReturnFalse(this LogCall<bool> logCall, string message) => logCall?.Return(false, message) ?? false;
    }
}
