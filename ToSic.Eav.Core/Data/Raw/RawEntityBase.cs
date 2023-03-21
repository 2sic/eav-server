using System;
using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Raw
{
    /// <summary>
    /// Base class for raw entities which makes implementations a bit simpler when not much is needed.
    /// For example, the dates default to now.
    ///
    /// You can inherit this class, but you can also just re-implement the interface yourself.
    /// Whatever works better for you.
    /// </summary>
    /// <remarks>
    /// Added in 15.04
    /// </remarks>
    [PublicApi]
    public abstract class RawEntityBase: IRawEntity, IHasRelationshipKeys
    {
        public virtual int Id { get; set; }
        public virtual Guid Guid { get; set; } = Guid.Empty;
        public virtual DateTime Created { get; set; } = DateTime.Now;
        public virtual DateTime Modified { get; set; } = DateTime.Now;
        public abstract Dictionary<string, object> Attributes(RawConvertOptions options);
        public virtual IEnumerable<object> RelationshipKeys(RawConvertOptions options) => new List<object>();
    }
}
