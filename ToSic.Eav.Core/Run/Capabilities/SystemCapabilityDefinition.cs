using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data.Raw;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Run.Capabilities
{
    public class SystemCapabilityDefinition: AspectDefinition, IHasRawEntity<IRawEntity>
    {
        public const string Prefix = "SystemCapability";

        public SystemCapabilityDefinition(string nameId, Guid guid, string name, string description = default, string link = default)
        : base($"{Prefix}/{nameId}", guid, name, description)
        {
            Link = link;
        }

        public string Link { get; set; }

        public static SystemCapabilityDefinition Blazor { get; } = new SystemCapabilityDefinition("Blazor", new Guid("9880cb15-ea2a-4b85-8eb8-7e9ccd390651"), "Blazor");

        public static SystemCapabilityDefinition Cs73 { get; } = new SystemCapabilityDefinition("Cs0703", new Guid("686f54b2-5464-4eed-8faf-c30a36899b42"), "C# 7.3");

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
