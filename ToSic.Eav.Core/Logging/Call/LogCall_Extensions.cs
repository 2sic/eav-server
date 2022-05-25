namespace ToSic.Eav.Logging
{
    public static class LogCall_Extensions
    {
        public static void Done(this LogCall logCall) => logCall?.DoneInternal(null);

        public static void Done(this LogCall logCall, string message) => logCall?.DoneInternal(message);
    }
}
