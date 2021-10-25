using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    [TestClass]
    public class ConvertToGuid: ConvertTestBase
    {
        //[TestMethod]
        //public void StringToGuid()
        //{
        //    AreEqual(null, (null as string).TestConvertOrDefault<string>());
        //    AreEqual("", "".TestConvertOrDefault<string>());
        //    AreEqual("5", "5".TestConvertOrDefault<string>());
        //}

        [TestMethod] public void NullToGuid() => ConvT(null, null as string, null);
        [TestMethod] public void EmptyStringToGuid() => ConvT("", Guid.Empty, Guid.Empty);
        [TestMethod] public void Number0ToGuid() => ConvT(0, Guid.Empty, Guid.Empty);
        [TestMethod] public void Number1ToGuid() => ConvT(1, Guid.Empty, Guid.Empty);

        [TestMethod] public void NullToGuidSimpleFallback() => ConvFbQuick(null, FbGuid, FbGuid, true, true);
        [TestMethod] public void NullToGuidDefaultFallback() => ConvFbQuick<Guid>(null, default, default, true, true);
        [TestMethod] public void StringEmptyDefaultFallback() => ConvFbQuick<Guid>("", default, default, true, true);
        [TestMethod] public void StringEmptySimpleFallback() => ConvFbQuick("", FbGuid, FbGuid, true, true);
        [TestMethod] public void StringValidSimpleFallback() => ConvFbQuick(StrGuid, FbGuid, ExpGuid, true, true);

        [TestMethod] public void NullToGuidOwnFallback() => ConvFbQuick(ExpGuid, default, true, true);
        // todo: test fallback!

        private const string StrGuid = "23cec5c7-3d54-43ef-a80a-e5e5c1f8a397";
        private static readonly Guid ExpGuid = new Guid(StrGuid);
        private static readonly Guid FbGuid = new Guid("6d1f8424-af44-4a9b-a98d-ab9c14723072");

        [TestMethod] public void StringBracketsToGuid() => ConvT("{" + StrGuid + "}", ExpGuid, ExpGuid, ExpGuid);
        [TestMethod] public void StringSpacesToGuid() => ConvT(" " + StrGuid + " ", ExpGuid, ExpGuid, ExpGuid); 
        [TestMethod] public void StringCompactToGuid() => ConvT(StrGuid.Replace("-", ""), ExpGuid, ExpGuid, ExpGuid);
        [TestMethod] public void StringNoBracketsToGuid() => ConvT(StrGuid, ExpGuid, ExpGuid, ExpGuid);
    }
}
