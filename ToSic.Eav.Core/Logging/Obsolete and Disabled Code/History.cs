//using System;
//using ToSic.Eav.Documentation;

//namespace ToSic.Eav.Logging
//{
//    [PrivateApi]
//    public static class History
//    {
//        [Obsolete("Will be removed in 2sxc 15 - just kept temporarily in case external DLLs are using this. Please get a LogHistory object using DI and call Add() on that")]
//        public static void Add(string key, ILog log)
//        {
//            new Lib.Logging.History().Add(key, log.GetContents());
//            // TODO: ADD Obsolete WARNING IF THIS IS CALLED
//            //new LogHistory().Add(key, log);
//        }
//    }


//}
