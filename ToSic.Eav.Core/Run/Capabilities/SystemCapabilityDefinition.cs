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
        /// Clone constructor - optionally override some values
        /// </summary>
        public SystemCapabilityDefinition Clone(string noParamOrder = Parameters.Protector, 
            string nameId = default, Guid? guid = default, string name = default, string description = default, string link = default) =>
            new SystemCapabilityDefinition(nameId ?? NameId, guid ?? Guid, name ?? Name, description ?? Description, link ?? Link);

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


        public override string ToString() => $"{Prefix}: {Name} ({NameId} / {Guid})";
    }
}
