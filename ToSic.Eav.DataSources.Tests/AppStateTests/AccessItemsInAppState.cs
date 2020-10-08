using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.DataSourceTests.AppStateTests
{
    [TestClass]
    public class AccessItemsInAppState
    {
        public const int LargeAppId = 7; // todo
        public const int ItemToAccess = 10503; // todo
        public const int Repeats = 1000;

        [TestMethod]
        public void AccessOne1000Times()
        {
            var timer = new Stopwatch();
            timer.Start();

            var app = State.Get(LargeAppId);
            for (var i = 0; i < Repeats; i++)
            {
                var found = app.List.One(ItemToAccess);
                if (found == null) throw new Exception("should never get null, IDs are probably wrong");
            }

            timer.Stop();
            Trace.Write($"Time used: {timer.ElapsedMilliseconds}");
        }

        [TestMethod]
        public void FastAccessOne1000Times()
        {
            var timer = new Stopwatch();
            timer.Start();

            var app = State.Get(LargeAppId);
            for (var i = 0; i < Repeats; i++)
            {
                var found = app.LFA.Get(ItemToAccess); //.FastOne(ItemToAccess);
                if (found == null) throw new Exception("should never get null, IDs are probably wrong");
            }

            timer.Stop();
            Trace.Write($"Time used: {timer.ElapsedMilliseconds}");
        }

    }
}
