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

        private const string CtAuthor = "Author";
        private const string FFullName = "FullName";
        private const string FKey = "Key";
        private const string FKeyInherited = "KeyInherited";

        [TestMethod]
        public void LoadCtAndCheckAttributeGuids()
        {
            var cts = LoadAllTypes();
            var author = cts.First(ct => ct.Name.Equals(CtAuthor));

            var attFullName = author[FFullName];
            IsNull(attFullName.Guid);

            var attKey = author[FKey];
            IsNotNull(attKey.Guid);
            AreEqual(AttributeShareLevel.SameApp, attKey.SysSettings.ShareLevel);

            var attKeyInherited = author[FKeyInherited];
            IsNotNull(attKeyInherited.Guid);
            AreEqual(AttributeShareLevel.None, attKeyInherited.SysSettings.ShareLevel);
            AreEqual(attKey.Guid, attKeyInherited.SysSettings.SourceGuid);
            IsTrue(attKeyInherited.SysSettings.InheritMetadata);
            IsFalse(attKeyInherited.SysSettings.InheritName);
        }

        [TestMethod]
        public void ReExportCtAndPreserveAttributeGuid()
        {
            var cts = LoadAllTypes();
            var author = cts.First(ct => ct.Name.Equals(CtAuthor));

            var attFullName = author[FFullName];
            IsNull(attFullName.Guid);

            var attKey = author[FKey];
            IsNotNull(attKey.Guid);
            
            // Re-json the author
            var ser = SerializerOfApp(Constants.PresetAppId);
            var json = ser.ToJson(author);
            var jsonFullName = json.Attributes.First(a => a.Name.Equals(FFullName));
            IsNull(jsonFullName.Guid);

            var jsonKey = json.Attributes.First(a => a.Name.Equals(FKey));
            IsNotNull(jsonKey.Guid);
            AreEqual(AttributeShareLevel.SameApp, jsonKey.SysSettings.ShareLevel);

            var jsonKeyInherited = json.Attributes.First(a => a.Name.Equals(FKeyInherited));
            IsNotNull(jsonKeyInherited.Guid);
            AreEqual(AttributeShareLevel.None, jsonKeyInherited.SysSettings.ShareLevel);
            AreEqual(attKey.Guid, jsonKeyInherited.SysSettings.SourceGuid);
            IsTrue(jsonKeyInherited.SysSettings.InheritMetadata);
            IsFalse(jsonKeyInherited.SysSettings.InheritName);

            // Re-content-type
            var author2 = ser.DeserializeContentType(ser.Serialize(author));
            attFullName = author2[FFullName];
            IsNull(attFullName.Guid);
            attKey = author2[FKey];
            IsNotNull(attKey.Guid);
            AreEqual(AttributeShareLevel.SameApp, attKey.SysSettings.ShareLevel);

            var attKeyInherited = author2[FKeyInherited];
            IsNotNull(attKeyInherited.Guid);
            AreEqual(AttributeShareLevel.None, attKeyInherited.SysSettings.ShareLevel);
            AreEqual(attKey.Guid, attKeyInherited.SysSettings.SourceGuid);
            IsTrue(attKeyInherited.SysSettings.InheritMetadata);
            IsFalse(attKeyInherited.SysSettings.InheritName);
        }

    }
}
