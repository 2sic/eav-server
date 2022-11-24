//namespace ToSic.Eav.Logging
//{
//    public static class LogCall_TExtensions
//    {
//        #region Return Statements - can't be used, because on LogCall<dynamic> they will fail - error "extension methods cannot be dynamically dispatched"

//        //public static T Return<T>(this LogCall<T> logCall, T result, string message)
//        //{
//        //    logCall?.DoneInternal(message);
//        //    return result;
//        //}

//        //public static T Return<T>(this LogCall<T> logCall, T result) => logCall.Return(result, null);


//        //public static T ReturnAndLog<T>(this LogCall<T> logCall, T result) => logCall.Return(result, $"{result}");


//        //public static T ReturnNull<T>(this LogCall<T> logCall) => logCall.Return(default, null);

//        //public static T ReturnNull<T>(this LogCall<T> logCall, string message) => logCall.Return(default, message);
//        #endregion

//        // TODO: @STV - pls use this instead of most calls where "ok" is in the text

//        public static T ReturnAsOk<T>(this LogCall<T> logCall, T result) => logCall == null ? result : logCall.Return(result, "ok");




//    }
//}
