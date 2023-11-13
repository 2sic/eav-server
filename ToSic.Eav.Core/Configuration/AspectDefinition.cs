using System;
using ToSic.Lib.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// Base class for various aspects of the system, such as features or capabilities.
    /// </summary>
    [PrivateApi("no good reason to publish this")]
    public class AspectDefinition: IHasIdentityNameId
    {
        protected AspectDefinition(string nameId, Guid guid, string name, string description = default)
        {
            NameId = nameId;
            Guid = guid;
            Name = name;
            Description = description;
        }

        public static AspectDefinition Custom(string nameId, Guid guid, string name = default, string description = default) 
            => new AspectDefinition(nameId, guid, name, description);

        public static AspectDefinition None = new AspectDefinition("None", Guid.Empty, "None");

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

        public override string ToString() => $"Aspect: {Name} ({NameId} / {Guid})";
    }
}
