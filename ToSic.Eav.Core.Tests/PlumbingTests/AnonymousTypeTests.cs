using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ToSic.Eav.Plumbing;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.Core.Tests.PlumbingTests
{
    [TestClass]
    public class AnonymousTypeTests
    {
        [TestMethod]
        public void SimpleValues()
        {
            IsFalse("".IsAnonymous());
            IsFalse(5.IsAnonymous());
            IsFalse((null as string).IsAnonymous());
            IsFalse(DateTime.Now.IsAnonymous());
            IsFalse(Guid.Empty.IsAnonymous());
            IsFalse(new string[0].IsAnonymous());
        }
        public void SimpleValueTypes()
        {
            IsFalse("".GetType().IsAnonymous());
            IsFalse(5.GetType().IsAnonymous());
            IsFalse((null as string).GetType().IsAnonymous());
            IsFalse(DateTime.Now.GetType().IsAnonymous());
            IsFalse(Guid.Empty.GetType().IsAnonymous());
            IsFalse(new string[0].GetType().IsAnonymous());
        }

        [TestMethod]
        public void RealObjects()
        {
            IsFalse(new List<string>().IsAnonymous());
            IsFalse(new AnonymousTypeTests().IsAnonymous());
            IsFalse(new Exception().IsAnonymous());
        }

        [TestMethod]
        public void AnonymousObjects()
        {
            var anon = new
            {
                Key = 27,
                Sub = new
                {
                    Key = 42
                },
                Guid = new Guid()
            };
            IsTrue(anon.IsAnonymous());
            IsTrue(anon.Sub.IsAnonymous());
            IsFalse(anon.Guid.IsAnonymous());
        }

        [TestMethod]
        public void FromJsonAlwaysDictionaries()
        {
            var jsonArray = "[5,3,4]";
            IsFalse(JsonConvert.DeserializeObject(jsonArray).IsAnonymous());

            var jsonObject = "{ \"key\": 27, \"key2\": 42 }";
            IsFalse(JsonConvert.DeserializeObject(jsonObject).IsAnonymous());

        }
    }
}
