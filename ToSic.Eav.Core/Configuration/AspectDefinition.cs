using System;
using ToSic.Lib.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// Base class for various aspects of the system, such as features or capabilities.
    /// </summary>
    [PrivateApi("no good reason to publish this")]
    public abstract class AspectDefinition: IHasIdentityNameId
    {
        protected AspectDefinition(string nameId, Guid guid, string name, string description = default)
        {
            NameId = nameId;
            Guid = guid;
            Name = name;
            Description = description;
        }

        /// <summary>
        /// GUID Identifier for this Aspect.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// String Identifier for this Aspect.
        /// </summary>
        public string NameId { get; }

        /// <summary>
        /// A nice name / title for showing in UIs
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A nice description
        /// </summary>
        public string Description { get; }
    }
}
