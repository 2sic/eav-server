using System;
using System.Collections.Generic;

namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class ToSicEavValuesDimensions
    {
        public int ValueId { get; set; }
        public int DimensionId { get; set; }
        public bool ReadOnly { get; set; }

        public virtual ToSicEavDimensions Dimension { get; set; }
        public virtual ToSicEavValues Value { get; set; }
    }
}
