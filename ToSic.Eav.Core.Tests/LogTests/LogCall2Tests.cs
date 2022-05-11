﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Core.Tests.LogTests
{
    [TestClass]
    public class LogCall2Tests
    {
        [TestMethod]
        public void NoReturnBasic()
        {
            var log = new Log("Test");
            var call = log.Call2();
            Assert.AreEqual(1, log.Entries.Count);
            call.Done("ok");
            
            Assert.AreEqual(2, log.Entries.Count); // Another for results
            var resultEntry = log.Entries.First();
            Assert.AreEqual("ok", resultEntry.Result);
        }

        [TestMethod]
        public void NoReturnAll()
        {
            var log = new Log("Test");
            var call = log.Call2($"something: {7}", "start msg", true);
            Assert.IsTrue(call.Stopwatch.ElapsedMilliseconds < 1);
            Assert.AreEqual(1, log.Entries.Count);
            System.Threading.Thread.Sleep(10); // wait 10 ms
            call.Done("ok");

            Assert.IsTrue(call.Stopwatch.ElapsedMilliseconds > 9);
            
            Assert.AreEqual(2, log.Entries.Count); // Another for results
            var resultEntry = log.Entries.First();
            Assert.AreEqual("ok", resultEntry.Result);
        }

        [TestMethod]
        public void GenericBasic()
        {
            var log = new Log("Test");
            var call = log.Call2<string>();
            
            Assert.AreEqual(1, log.Entries.Count);  // Should have one when starting
            var result = call.Done("ok", "result");
            Assert.AreEqual("result", result);

            Assert.AreEqual(2, log.Entries.Count);  // Another for results
            var resultEntry = log.Entries.First();
            Assert.AreEqual("ok", resultEntry.Result);
        }
    }
}
