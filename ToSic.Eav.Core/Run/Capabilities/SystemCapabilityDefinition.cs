using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Run.Capabilities
{
    public class SystemCapabilityDefinition: AspectDefinition, IHasRawEntity<IRawEntity>
    {
        public const string Prefix = "SystemCapability";

        public SystemCapabilityDefinition(string nameId, Guid guid, string name, string description = default, string link = default)
        : base(EnsurePrefix(nameId), guid, name, description)
        {
            Link = link;
        }

        /// <summary>
        /// Clone constructor - only original is required
        /// </summary>
        private SystemCapabilityDefinition(SystemCapabilityDefinition original, string nameId = default, Guid? guid = default, string name = default, string description = default, string link = default)
        : base(EnsurePrefix(nameId ?? original.NameId), guid ?? original.Guid, name ?? original.Name, description ?? original.Description)
        {
            Link = link ?? original.Link;
        }

        public SystemCapabilityDefinition Clone(string noParamOrder = Parameters.Protector, 
            string nameId = default, Guid? guid = default, string name = default, string description = default, string link = default)
            => new SystemCapabilityDefinition(this, nameId, guid, name, description, link);

        private static string EnsurePrefix(string original) 
            => original.IsEmptyOrWs() ? Prefix + "-Error-No-Name" : original.StartsWith(Prefix) ? original : $"{Prefix}-{original}";

        public string Link { get; set; }

        public IRawEntity RawEntity => _newEntity.Get(() => new RawEntity
        {
            Guid = Guid,
            Values = new Dictionary<string, object>
            {
                {nameof(NameId), NameId},
                {nameof(Name), Name},
                {nameof(Description), Description},
                {nameof(Link), Link},
            }
        });

        private readonly GetOnce<IRawEntity> _newEntity = new GetOnce<IRawEntity>();

    }
}
