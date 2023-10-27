using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests
{
    [TestClass]
    public class FileLoaderCtAttrGuid : FileLoaderCtBase
    {
        private const string DataFolder = "attrib-guids";
        private const string CtAuthor = "AuthorWithSharedAttributes";
        private const string FFullName = "FullName";
        private const string FKey = "Key";
        private const string FKeyInherited = "KeyInherited";
        private static Guid SourceKeyGuid = new Guid("4e7164a1-72f2-4dee-97ea-55362e55e635");

        [TestMethod]
        public void LoadCtAndCheckAttributeGuids()
        {
            var cts = LoadAllTypes(DataFolder);
            var author = cts.First(ct => ct.Name.Equals(CtAuthor));

            VerifyNormalFullName(author);
            VerifySpecialKey(author);
            VerifySpecialKeyInherited(author);
        }

        private static void VerifySpecialKeyInherited(IContentType author)
        {
            var attKeyInherited = author[FKeyInherited];
            IsNotNull(attKeyInherited.Guid);
            IsFalse(attKeyInherited.SysSettings.Share);
            AreEqual(SourceKeyGuid, attKeyInherited.SysSettings.InheritMetadataMainGuid);
            IsTrue(attKeyInherited.SysSettings.InheritMetadata);
            IsFalse(attKeyInherited.SysSettings.InheritNameOfPrimary);
        }

        private static void VerifyNormalFullName(IContentType author)
        {
            // Check an attribute which shouldn't have a guid
            var attFullName = author[FFullName];
            IsNull(attFullName.Guid);
            IsNull(attFullName.SysSettings);
        }

        private static void VerifySpecialKey(IContentType author)
        {
            // Verify the Key has a GUID
            var attKey = author[FKey];
            IsNotNull(attKey.Guid);
            AreEqual(SourceKeyGuid, attKey.Guid);
            IsTrue(attKey.SysSettings.Share);
            IsFalse(attKey.SysSettings.InheritMetadata);
        }

        [TestMethod]
        public void ReExportCtAndPreserveAttributeGuid()
        {
            var cts = LoadAllTypes(DataFolder);
            var author = cts.First(ct => ct.Name.Equals(CtAuthor));

            VerifyNormalFullName(author);
            VerifySpecialKey(author);
            VerifySpecialKeyInherited(author);

            // Re-json the author and check
            var ser = SerializerOfApp(Constants.PresetAppId);
            var json = ser.ToJson(author);
            var jsonFullName = json.Attributes.First(a => a.Name.Equals(FFullName));
            IsNull(jsonFullName.Guid);

            var jsonKey = json.Attributes.First(a => a.Name.Equals(FKey));
            IsNotNull(jsonKey.Guid);
            AreEqual(SourceKeyGuid, jsonKey.Guid);
            IsTrue(jsonKey.SysSettings.Share);

            var jsonKeyInherited = json.Attributes.First(a => a.Name.Equals(FKeyInherited));
            IsNotNull(jsonKeyInherited.Guid);
            //AreEqual(0, jsonKeyInherited.SysSettings.ShareLevel);
            AreEqual(SourceKeyGuid.ToString(), jsonKeyInherited.SysSettings.InheritMetadataOf);
            IsTrue(jsonKeyInherited.SysSettings.InheritMetadata);
            IsFalse(jsonKeyInherited.SysSettings.InheritName);

            // Re-content-type
            var author2 = ser.DeserializeContentType(ser.Serialize(author));
            VerifyNormalFullName(author2);
            VerifySpecialKey(author2);
            VerifySpecialKeyInherited(author2);
        }

    }
}
