using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.DataSourceTests.ExternalData;

namespace ToSic.Eav.DataSourceTests.Attributes
{
    [TestClass]
    public partial class AttributeRenameTests
    {
        
        private static void AssertHasFields(IEntity item, IEnumerable<string> fieldsExpected)
        {
            foreach (var f in fieldsExpected) Assert.IsTrue(item.Attributes.ContainsKey(f), $"should have field '{f}'");
        }

        private static void AssertDoesNotHaveFields(IEntity item, IEnumerable<string> fieldsExpected)
        {
            foreach (var f in fieldsExpected) Assert.IsFalse(item.Attributes.ContainsKey(f), $"should have field '{f}'");
        }

        private static List<string> ChangeFieldList(string[] removeFields, string[] addFields)
        {
            var changedFields = DataTableTst.Fields.ToList();

            foreach (var r in removeFields) changedFields.Remove(r);
            foreach (var a in addFields) changedFields.Add(a);
            return changedFields;
        }

        private static void AssertFieldsChanged(IEntity item, string[] removedFields, string[] addFields)
        {
            var changedFields = ChangeFieldList(removedFields, addFields);
            AssertHasFields(item, changedFields);
            AssertDoesNotHaveFields(item, removedFields);
        }

    }
}
