﻿// 2018-03-09 2dm disabled various tests related to code-based content-types, which ATM are never in use

namespace ToSic.Eav.Core.Tests.Types;

public class GlobalManagerTests
{
    public const int CodeTypesCount = 0; // 2018-03-09 no more code-types provided

    // 2018-06-25 2dm - as of now, there are not code-based content-types
    //[TestMethod]
    //public void ScanForTypesReflection()
    //{
    //    var globTypes = Global.ContentTypesInReflection();
    //    Assert.AreEqual(CodeTypesCount, globTypes.Count(), "expect a fixed about of types at dev time");
    //}



    //[TestMethod]
    //public void TestGlobalCache()
    //{
    //    var all = Global.AllContentTypes();
    //    Assert.AreEqual(ProvidedTypesCount, all.Count);
    //    var dummy = all.First();
    //    Assert.AreEqual(DemoType.CTypeName, dummy.Key);
    //}

    //[TestMethod]
    //public void TestInstanceInGlobalCache()
    //{
    //    var dummy = Global.FindContentType(DemoType.CTypeName);
    //    Assert.AreEqual(DemoType.CTypeName, dummy.StaticName);
    //}

    //[TestMethod]
    //public void CheckDefaultScope()
    //{
    //    var testType = Global.FindContentType(DemoType.CTypeName);
    //    Assert.AreEqual(TypesBase.UndefinedScope, testType.Scope, "scope should be undefined");
    //}

    //[TestMethod]
    //[Ignore]
    //public void TestUndefinedTitle()
    //{
    //    // todo - must create an instance entity to test this

    //}

}