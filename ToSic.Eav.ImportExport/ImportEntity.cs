using System;
using System.Collections.Generic;

namespace ToSic.Eav.Import
{
    public class ImportEntity
    {
        public string AttributeSetStaticName { get; set; }
        public int? KeyNumber { get; set; }
        public Guid? KeyGuid { get; set; }
        public string KeyString { get; set; }
        public int AssignmentObjectTypeId { get; set; }
        public Guid? EntityGuid { get; set; }
        public bool IsPublished { get; set; }
        public Dictionary<string, List<IValueImportModel>> Values { get; set; }

        public ImportEntity()
        {
            IsPublished = true;
        }
    }
}