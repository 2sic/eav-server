﻿namespace ToSic.Eav.Logging
{
    public static class LogCall_BoolExtensions
    {

        // TODO: @STV - pls use this instead of most calls where true/  false is in the result

        public static bool ReturnTrue(this LogCall<bool> logCall) => logCall.ReturnTrue("");

        public static bool ReturnTrue(this LogCall<bool> logCall, string message) => logCall?.Return(true, $"{true} {message}") ?? true;

        public static bool ReturnFalse(this LogCall<bool> logCall) => logCall.ReturnFalse("");

        public static bool ReturnFalse(this LogCall<bool> logCall, string message) => logCall?.Return(false, $"{false} {message}") ?? false;

    }
}
