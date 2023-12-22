using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.AppStateTests
{
    [TestClass]
    public class AccessItemsInAppState: TestBaseDiEavFullAndDb
    {
        public const int ItemToAccess = 17000;
        public const int Repeats = 1000;

        [TestMethod]
        public void Access1Times()
        {
            var app = GetAppState();
            var timer = new Stopwatch();
            timer.Start();

            var found = app.List.One(ItemToAccess);
            if (found == null) throw new Exception("should never get null, IDs are probably wrong");

            timer.Stop();
            Trace.Write($"Time used: {timer.ElapsedMilliseconds}");
        }

        private IAppState GetAppState() => GetService<IAppStates>().GetReader(TestConfig.BigDataTestsApp);

        [TestMethod]
        public void AccessOne1000TimesSame() => AccessOne1000Times(0);

        [TestMethod]
        public void AccessOne1000TimesDiff() => AccessOne1000Times(1);


        public void AccessOne1000Times(int multNext)
        {
            var app = GetAppState();
            var timer = new Stopwatch();
            timer.Start();

            for (var i = 0; i < Repeats; i++)
            {
                var found = app.List.One(ItemToAccess + i * multNext);
                if (found == null) throw new Exception("should never get null, IDs are probably wrong");
            }

            timer.Stop();
            Trace.Write($"Time used: {timer.ElapsedMilliseconds}");
        }

        [TestMethod]
        public void FastAccessOne1000TimesLFASame() => AccessOneLazyFastAccess(0);

        [TestMethod]
        public void FastAccessOne1000TimesLFADiff() => AccessOneLazyFastAccess(1);

        public void AccessOneLazyFastAccess(int multiplyCounter)
        {
            var app = GetAppState();
            var timer = new Stopwatch();
            timer.Start();

            for (var i = 0; i < Repeats; i++)
            {
                var found = app.List.One(ItemToAccess + i * multiplyCounter);
                if (found == null) throw new Exception("should never get null, IDs are probably wrong");
            }

            timer.Stop();
            Trace.Write($"Time used: {timer.ElapsedMilliseconds}");
        }

        //[TestMethod]
        //public void AccessOne1000TimesFastIndexSame() => FastAccessOne1000TimesFastIndex(0);
        //[TestMethod]
        //public void AccessOne1000TimesFastIndexDiff() => FastAccessOne1000TimesFastIndex(1);

        //public void FastAccessOne1000TimesFastIndex(int multiplyCounter)
        //{
        //    var app = State.Get(new AppIdentity(TestConfig.Zone, TestConfig.AppForBigDataTests));
        //    var timer = new Stopwatch();
        //    timer.Start();

        //    for (var i = 0; i < Repeats; i++)
        //    {
        //        var found = app.FastOne(ItemToAccess + i * multiplyCounter); //.FastOne(ItemToAccess);
        //        if (found == null) throw new Exception("should never get null, IDs are probably wrong");
        //    }

        //    timer.Stop();
        //    Trace.Write($"Time used: {timer.ElapsedMilliseconds}");
        //}

    }
}
