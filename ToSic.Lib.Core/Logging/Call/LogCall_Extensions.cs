namespace ToSic.Lib.Logging
{
    // ReSharper disable once InconsistentNaming
    public static class LogCall_Extensions
    {
        public static void Done(this LogCall logCall) => logCall?.DoneInternal(null);

        public static void Done(this LogCall logCall, string message) => logCall?.DoneInternal(message);
    }
}
